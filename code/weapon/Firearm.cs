#nullable enable

using ClockBlockers.Timeline;
using Sandbox;

using System.Collections.Generic;
using System.Linq;


namespace ClockBlockers;

/// <summary>
/// A trace result with additional metadata attached.
/// </summary>
public struct ExtendedTraceResult
{
	public TraceResult Result { get; }

	public bool DidPenetrate { get; set; }

	public ExtendedTraceResult( TraceResult traceResult )
	{
		Result = traceResult;
	}
}

/// <summary>
/// A weapon that can shoot like a firearm.
/// Contains custom persistence code for traces.
/// </summary>
public abstract partial class Firearm : Weapon
{
	/// <summary>
	/// The distance a target entity is allowed to move between timelines before damage is recalculated.
	/// </summary>
	public virtual float PositionErrorMargin => 16f;

	/// <summary>
	/// The amount of damage inflicted by the default <c>CreateDamage</c> implementation.
	/// </summary>
	public virtual float BaseDamageAmount => 10f;

	/// <summary>
	/// Shoot this firearm. Only called for free agents.
	/// </summary>
	/// <param name="traces">The traces to use.</param>
	public void Shoot( params TraceInfo[] traces )
	{
		if ( Pawn == null ) return;

		LinkedList<PersistentTrace> persistentTraces = new();

		foreach ( var trace in traces )
		{
			PersistentTrace persistTrace = new()
			{
				TraceInfo = trace
			};

			LinkedList<EntityDamage> damagedEnts = new();
			foreach ( var tr in DoTrace( CreateTrace( trace ) ) )
			{
				using ( Prediction.Off() )
				{
					if (Game.IsServer)
					{
						DamageInfo damageInfo = CreateDamage( tr );
						var entDamage = EntityDamage.FromDamageInfo( damageInfo, tr.Result.Entity );
						entDamage.DidPenetrate = tr.DidPenetrate;

						damagedEnts.AddLast( entDamage );
						tr.Result.Entity.TakeDamage( damageInfo );
					}
				}
			}

			persistTrace.DamagedEntities.AddRange( damagedEnts );
			persistentTraces.AddLast( persistTrace );
		}

		using ( Prediction.Off() )
		{
			var capture = Pawn.AnimCapture;
			if ( capture != null )
			{
				ShootAction action = new ShootAction();
				action.Traces.AddRange( persistentTraces );
				capture.AddAction( action );
			}
		}

		DoShootEffects( traces );
	}

	/// <summary>
	/// Called when this weapon is shot by a remnant.
	/// </summary>
	/// <param name="action">The instigating shoot action.</param>
	public void ShootRemnant( ShootAction action )
	{
		foreach ( var trace in action.Traces )
		{
			ShootRemnantTrace( trace );
		}
		DoShootEffects( action.Traces.Select( tr => tr.TraceInfo ) );
	}

	// Some big-brain programming here
	private void ShootRemnantTrace( PersistentTrace trace )
	{
		if ( Pawn == null ) return;

		// All entity damage in the original trace that haven't been disrupted.
		var recordedEntDamage = trace.DamagedEntities.AsEnumerable().Where( IsEntityConsistent ).ToList();

		// All entity damage in the original trace that haven't been disrupted.
		var recordedEntities = recordedEntDamage.Select( dmg => PersistentEntities.GetEntity<Entity>( dmg.Target ) ).Where( ent => ent != null ).ToHashSet();

		// If the recorded damage did not penetrate its final hit, we stop the new trace at that point.
		Vector3? cullDistance = null;
		if ( recordedEntDamage.Any() && !recordedEntDamage.Last().DidPenetrate )
		{
			cullDistance = recordedEntDamage.Last().HitPosition;
		}

		//bool cullTrace = false;
		//if ( recordedEntDamage.Any() )
		//{
		//	cullTrace = recordedEntDamage.Last().DidPenetrate == false;
		//}

		// Perform a new trace and remove entities that are in the old trace or are past the cull distance.
		var newTrace = DoTrace( CreateTrace( trace.TraceInfo ) )
			.Where( tr => !recordedEntities.Contains( tr.Result.Entity ) )
			.Where( tr => !cullDistance.HasValue || tr.Result.HitPosition.DistanceSquared( trace.TraceInfo.Start ) > cullDistance.Value.Distance( trace.TraceInfo.Start ) + 64 )
			.ToList();

		// A new entity might have obstructed the bullet; we need to cull again.
		if ( newTrace.Any() && !newTrace.Last().DidPenetrate )
		{
			var newCullDistance = newTrace.Last().Result.HitPosition;
			recordedEntDamage = recordedEntDamage.Where( dmg => dmg.HitPosition.DistanceSquared( trace.TraceInfo.Start ) > newCullDistance.Distance( trace.TraceInfo.Start ) + 32 ).ToList();
		}

		foreach ( var dmg in recordedEntDamage )
		{
			var damage = dmg.ToDamageInfo( Pawn, out var target );
			if ( damage.HasValue && target != null ) target.TakeDamage( damage.Value );
		}

		foreach ( var tr in newTrace )
		{
			var damage = CreateDamage( tr );
			tr.Result.Entity.TakeDamage( damage );
		}
	}

	private Trace CreateTrace( TraceInfo traceInfo )
	{
		return traceInfo.CreateTrace().Ignore( this );
	}

	private bool IsEntityConsistent( EntityDamage entDamage )
	{
		var entity = PersistentEntities.GetEntity<Entity>( entDamage.Target );
		if ( entity == null ) return false;

		return entity.Position.DistanceSquared( entDamage.TargetPosition ) <= PositionErrorMargin * PositionErrorMargin;
	}

	/// <summary>
	/// Create an appropriate <c>DamageInfo</c> from a trace result.
	/// </summary>
	/// <param name="tr">The trace result.</param>
	/// <returns>The damage.</returns>
	protected DamageInfo CreateDamage( ExtendedTraceResult tr )
	{
		return DamageInfo.FromBullet( tr.Result.EndPosition, 0, BaseDamageAmount )
					.UsingTraceResult( tr.Result )
					.WithAttacker( Owner )
					.WithWeapon( this );
	}

	/// <summary>
	/// Create sounds and other visual effects related to shooting. Called for free agents AND remnants.
	/// </summary>
	/// <param name="traces">The traces that were shot.</param>
	public abstract void DoShootEffects( IEnumerable<TraceInfo> traces );

	/// <summary>
	/// Perform a trace.
	/// </summary>
	/// <param name="trace">The trace request.</param>
	/// <returns>Trace results. All results must be on the ray of the trace,
	/// and must be in order from closest to farthest.</returns>
	protected virtual IEnumerable<ExtendedTraceResult> DoTrace( Trace trace )
	{
		var tr = trace.Run();

		if ( tr.Hit )
			yield return new ExtendedTraceResult( tr );
	}

}
