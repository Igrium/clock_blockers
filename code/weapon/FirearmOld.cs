//#nullable enable

//using ClockBlockers.Anim;
//using ClockBlockers.Timeline;
//using Sandbox;
//using Sandbox.UI;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Threading.Tasks;

//namespace ClockBlockers;

///// <summary>
///// A weapon that can be shot like a gun (contains dedicated trace code)
///// </summary>
//public abstract class FirearmOld : Weapon
//{
//	/// <summary>
//	/// The distance a target entity is allowed to move between timelines before damage is recalculated.
//	/// </summary>
//	public virtual float PositionErrorMargin => 16f;

//	public virtual float DamageAmount => 10f;

//	public void Shoot( params PersistentTrace[] traces )
//	{
//		if ( Pawn == null ) return;

//		foreach ( var trace in traces )
//		{
//			LinkedList<EntityDamage> damagedEnts = new();
//			foreach ( var tr in DoTrace( trace.CreateTrace() ) )
//			{
//				using (Prediction.Off())
//				{
//					DamageInfo damage = CreateDamageInfo( tr );
//					damagedEnts.AddLast( EntityDamage.FromDamageInfo( damage, tr.Entity ) );
//				}

//			}

//			trace.DamagedEntities.AddRange( damagedEnts );
//		}

//		using (Prediction.Off())
//		{
//			var capture = Pawn.AnimCapture;
//			if ( capture != null )
//			{
//				ShootAction action = new ShootAction();
//				action.Traces.AddRange( traces );
//				capture.AddAction( action );
//			}
//		}

//		DoShootEffects( traces );
//	}


//	/// <summary>
//	/// Called when this weapon is shot by a remnant.
//	/// </summary>
//	/// <param name="action">The instigating shoot action.</param>
//	public void ShootRemnant( ShootAction action )
//	{
//		foreach ( var trace in action.Traces )
//		{
//			ShootRemnantTrace( trace );
//		}

//		DoShootEffects( action.Traces );
//	}

//	// TODO: this might still have issues with penetration traces and penetration count.
//	// This method is some big brain programming. I might need to re-think some of the logic.
//	private void ShootRemnantTrace( PersistentTrace trace )
//	{
//		if ( Pawn == null ) return;

//		var recordedEntDamage = trace.DamagedEntities.AsEnumerable().Where( IsEntityConsistent ).ToHashSet();
//		var recordedEntities = new HashSet<Entity>();
//		foreach ( var dmg in recordedEntDamage )
//		{
//			var ent = PersistentEntities.GetEntity<Entity>( dmg.Target );
//			if ( ent != null ) recordedEntities.Add( ent );
//		}

//		// The recordwed entity with the greatest distance from the camera.
//		EntityDamage? lastRecordedEntity = null;
//		foreach ( var dmg in trace.DamagedEntities )
//		{
//			if ( !lastRecordedEntity.HasValue || dmg.HitPosition.DistanceSquared( trace.Start ) > lastRecordedEntity.Value.HitPosition.Distance( trace.Start ) )
//			{
//				lastRecordedEntity = dmg;
//			}
//		}

//		if ( lastRecordedEntity.HasValue && !IsEntityConsistent( lastRecordedEntity.Value ) )
//			lastRecordedEntity = null;

//		var newTrace = DoTrace( trace.CreateTrace() )
//			.Where( tr => !recordedEntities.Contains( tr.Entity ) )
//			// Remove new trace results if the old final trace result is still valid and closer than the new one.
//			.Where( tr => !(lastRecordedEntity.HasValue && tr.HitPosition.DistanceSquared( trace.Start ) > lastRecordedEntity.Value.HitPosition.DistanceSquared( trace.Start )) );

//		// The farthest distance the bullet traveled.
//		Vector3 traceLimit = trace.End;

//		// Cull the trace limit to the last hit position of the new trace.
//		// This will inturrupt the trace if there's a new entity blocking it.
//		// If there's a recorded entity closer than this new position,
//		// it won't have any effect.
//		if ( newTrace.Count() > 0 )
//			traceLimit = newTrace.Last().HitPosition;

//		// Filter entities that are significantly farther from new end position( shot was probably obstructed )
//		recordedEntDamage.RemoveWhere( dmg => dmg.HitPosition.DistanceSquared( trace.Start ) >= traceLimit.DistanceSquared( trace.Start ) + (64 ^ 2) );

//		foreach ( var dmg in recordedEntDamage )
//		{
//			var damage = dmg.ToDamageInfo( Pawn, out var target );
//			if ( damage.HasValue && target != null ) target.TakeDamage( damage.Value );
//		}

//		foreach ( var tr in newTrace )
//		{
//			var damage = CreateDamageInfo( tr );
//			tr.Entity.TakeDamage( damage );
//		}
//	}

//	/// <summary>
//	/// Create the <c>DamageInfo</c> object for each bullet. Does not actually inflict damage.
//	/// Only called for free agents; saved into animation for remnants.
//	/// </summary>
//	/// <param name="tr">The trace result to use.</param>
//	/// <returns>The damage info.</returns>
//	public virtual DamageInfo CreateDamageInfo( TraceResult tr )
//	{
//		return DamageInfo.FromBullet( tr.EndPosition, 0, DamageAmount )
//					.UsingTraceResult( tr )
//					.WithAttacker( Owner )
//					.WithWeapon( this );
//	}

//	private bool IsEntityConsistent( EntityDamage entDamage )
//	{
//		var entity = PersistentEntities.GetEntity<Entity>( entDamage.Target );
//		if ( entity == null ) return false;

//		return entity.Position.Distance( entDamage.TargetPosition ) <= PositionErrorMargin;
//	}

//	/// <summary>
//	/// Create sounds, particle effects, etc relating to shooting the weapon.
//	/// </summary>
//	/// <param name="traces">The traces</param>
//	public abstract void DoShootEffects( IEnumerable<PersistentTrace> traces );

//	/// <summary>
//	/// Perform a trace. Caled for remnants AND free agents.
//	/// Override this if your bullet has custom behavior upon hitting something
//	/// (can penetrate entities, etc.)
//	/// </summary>
//	/// <param name="trace">The trace configuration</param>
//	/// <returns>All of the entities hit, in hit order.</returns>
//	protected virtual IEnumerable<TraceResult> DoTrace( Trace trace )
//	{
//		var tr = trace.Run();
//		if ( tr.Hit )
//			yield return tr;
//	}

//	/// <summary>
//	/// Setup the parameters for a bullet trace. Only called for free agents.
//	/// </summary>
//	/// <param name="start">Trace start.</param>
//	/// <param name="end">Trace end.</param>
//	/// <param name="radius">Radius of the trace.</param>
//	/// <returns>The trace configuration object</returns>
//	protected virtual PersistentTrace CreateTrace( Vector3 start, Vector3 end, float radius = 2.0f )
//	{
//		bool underWater = Trace.TestPoint( start, "water" );

//		var persistentID = Owner.GetPersistentID( generate: true );
//		if ( persistentID == null )
//		{
//			throw new InvalidOperationException( "Failed to generate persistent ID" );
//		}

//		var trace = new PersistentTrace()
//		{
//			Start = start,
//			End = end,
//			Ignore = persistentID,
//			Radius = radius
//		};

//		trace.WithAnyTags( "solid", "player", "npc", "glass" );
//		if ( underWater )
//		{
//			trace.WithAnyTags( "water" );
//		}

//		//var trace = Trace.Ray( start, end )
//		//		.UseHitboxes()
//		//		.WithAnyTags( "solid", "player", "npc", "glass" )
//		//		.Ignore( this )
//		//		.Size( radius );

//		////
//		//// If we're not underwater then we can hit water
//		////
//		//if ( !underWater )
//		//	trace = trace.WithAnyTags( "water" );

//		return trace;
//	}

//	protected virtual PersistentTrace CreateBulletTrace( Vector3 pos, Vector3 dir, float spread, float bulletSize )
//	{
//		var forward = dir;
//		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
//		forward = forward.Normal;

//		var end = forward * 5000;

//		return CreateTrace( pos, end, bulletSize );
//	}


//	protected virtual PersistentTrace CreateBulletTrace( float spread, float bulletSize )
//	{
//		var ray = Owner.AimRay;
//		return CreateBulletTrace(ray.Position, ray.Forward, spread, bulletSize );
//	}
//}
