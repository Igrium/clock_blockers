#nullable enable

using ClockBlockers.Anim;
using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

/// <summary>
/// Records a pawn firing a weapon.
/// </summary>
public class ShootAction : IAction
{
	public List<PersistentTrace> Traces { get; private set; } = new();

	public void Run( AgentPawn pawn )
	{
		if ( pawn.ActiveWeapon is Firearm weapon )
			weapon.ShootRemnant( this );
	}
}

/// <summary>
/// A simple container of info required to perform a trace.
/// </summary>
public struct TraceInfo
{
	public Vector3 Start { get; set; }
	public Vector3 End { get; set; }
	public string[] Tags { get; set; }
	public float Radius { get; set; }

	public TraceInfo WithAnyTags( params string[] tags )
	{
		Tags = Tags.Concat( tags ).ToArray();
		return this;
	}

	/// <summary>
	/// Create a "real" trace from this persistent trace.
	/// </summary>
	/// <returns>The trace.</returns>
	public Trace CreateTrace()
	{
		return Trace.Ray( Start, End )
			.UseHitboxes()
			.WithAnyTags( Tags )
			.Size( Radius );
	}


}

/// <summary>
/// A bullet trace that gets recorded into the animation.
/// Contains the data to recreate the trace as well as a list of damaged entities.
/// </summary>
public class PersistentTrace
{
	public TraceInfo TraceInfo { get; set; }
	/// <summary>
	/// The entities damaged by this trace.
	/// </summary>
	public List<EntityDamage> DamagedEntities { get; protected set; } = new();

}

/// <summary>
/// A simplified damage struct that represents damage taken from a bullet.
/// </summary>
public struct EntityDamage
{
	/// <summary>
	/// Amount of damage to take.
	/// </summary>
	public float Damage { get; set; }

	/// <summary>
	/// Persistent ID of the target entity.
	/// </summary>
	public string Target { get; set; }

	/// <summary>
	/// The location of the damage local to the target.
	/// </summary>
	public Vector3 HitPosition { get; set; }

	/// <summary>
	/// The index of the bone that was hit.
	/// </summary>
	public int BoneIndex { get; set; }

	/// <summary>
	/// The hitbox that was hit.
	/// Because hitboxes are structs with no object references, we can store it here.
	/// </summary>
	public Hitbox Hitbox { get; set; }

	/// <summary>
	/// The amount of force applied by the damage.
	/// </summary>
	public Vector3 Force { get; set; }

	/// <summary>
	/// The true (not lag corrected) position the target was at when hit. Used to check for deviation from original timeline during replay.
	/// </summary>
	public Vector3 TargetPosition { get; set; }

	/// <summary>
	/// Whether this bullet penetrated the target.
	/// </summary>
	public bool DidPenetrate { get; set; }

	public HashSet<string> Tags { get; set; }

	/// <summary>
	/// Create an <c>EntityDamage</c> from a <c>DamageInfo</c>.
	/// </summary>
	/// <param name="damageInfo">The <c>DamageInfo</c> to use.</param>
	/// <param name="target">The target entity.</param>
	/// <returns>The new entity damage.</returns>
	/// <exception cref="ArgumentException">If the target entity has no persistent ID and one can't be created.</exception>
	public static EntityDamage FromDamageInfo( DamageInfo damageInfo, Entity target )
	{
		var persistentID = target.GetPersistentID( true );
		if ( persistentID == null )
		{
			throw new ArgumentException( "Entity does not have a persistent ID.", paramName: "target" );
		}

		return new()
		{
			Damage = damageInfo.Damage,
			Target = persistentID,
			HitPosition = target.Transform.PointToLocal( damageInfo.Position ),
			BoneIndex = damageInfo.BoneIndex,
			Hitbox = damageInfo.Hitbox,
			Force = damageInfo.Force,
			TargetPosition = target.Position,
			Tags = damageInfo.Tags
		};

	}

	/// <summary>
	/// Create a <c>DamageInfo</c> from this <c>EntityDamage</c>.
	/// </summary>
	/// <param name="instigator">The instigator entity</param>
	/// <param name="target">The entity the damage should be inflicted on.</param>
	/// <returns>The generated <c>DamageInfo</c>, or <c>null</c> if target wasn't found.</returns>
	public DamageInfo? ToDamageInfo( AgentPawn instigator, out Entity? target )
	{
		target = PersistentEntities.GetEntity<Entity>( Target );
		if ( target == null )
		{
			Log.Warning( $"No entity found with persistent ID '{Target}'." );
			return null;
		}


		return new()
		{
			Attacker = instigator,
			Weapon = instigator.ActiveWeapon,
			Position = target.Transform.PointToWorld( HitPosition ),
			Force = Force,
			Damage = Damage,
			Tags = Tags,
			BoneIndex = BoneIndex,
			Hitbox = Hitbox
		};
	}
}
