#nullable enable

using ClockBlockers.Anim;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

/// <summary>
/// A firearm that can shoot bullets.
/// Contains custom code for saving traces persistently.
/// </summary>
public abstract partial class BaseFirearm : Carriable
{

	public override bool CanDrop => true;

	/// <summary>
	/// If the fire effects on this weapon should act continuous (smg, etc)
	/// </summary>
	public virtual bool IsFireContinuous => false;

	public void FireBullets( IEnumerable<BulletInfo> bullets, bool isRemnant = false )
	{
		foreach ( var bullet in bullets )
			FireBullet( bullet, isRemnant );
	}


	/// <summary>
	/// Fire a bullet from this weapon.
	/// </summary>
	/// <param name="bullet">The bullet to shoot.</param>
	/// <param name="isRemnant">If this is being shot by a remnant.</param>
	public virtual void FireBullet(BulletInfo bullet, bool isRemnant = false)
	{
		Vector3 start = bullet.Ray.Position;
		Vector3 end = bullet.Ray.Project( 2048 );
		float radius = bullet.Radius;

		foreach (var tr in DoBulletTrace(start, end, radius))
		{
			if ( !tr.Hit ) continue;
			DealDamage( tr, bullet );
			tr.Surface.DoBulletImpact( tr );
		}

		if (Owner is Player player && player.IsRecording)
		{
			player.AnimCapture?.AddAction( new ShootAction( bullet ) );
		}
	}

	/// <summary>
	/// Perform a bullet trace.
	/// </summary>
	/// <param name="start">Trace start point</param>
	/// <param name="end">Trace end point</param>
	/// <param name="radius">Trace radius</param>
	/// <returns>All of the entities that were hit (or penetrated), in order.</returns>
	protected IEnumerable<TraceResult> DoBulletTrace( Vector3 start, Vector3 end, float radius = 2f )
	{
		var trace = Trace.Ray( start, end )
			.Radius( radius )
			.UseHitboxes()
			.Ignore( Owner )
			.WithAnyTags( "solid", "player", "npc", "penetrable", "corpse", "glass", "water", "carriable" )
			.WithoutTags( "trigger", "skybox", "playerclip" );

		foreach (var result in trace.RunAll())
		{
			yield return result;
			if ( !IsPenetrable( result.Entity ) )
				yield break;
		}
	}

	protected virtual void DealDamage( TraceResult tr, BulletInfo bullet )
	{
		using ( Prediction.Off() )
		{
			float damage = DamageFalloff.CalculateDamage( bullet.BaseDamage, tr.Distance, bullet.FalloffMultiplier, bullet.FalloffType );

			var dmgInfo = DamageInfo.FromBullet( tr.HitPosition, bullet.Ray.Forward * 100 * bullet.Force, damage )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithTag( "bullet" );

			tr.Entity.TakeDamage( dmgInfo );
		}
	}

	public virtual bool IsPenetrable( Entity entity )
	{
		return (entity.Tags.HasAny( new() { "penetrable", "water", "glass" } ) || entity is ShatterGlass || entity is GlassShard);
	}

	public virtual void DoShootEffects()
	{
		if ( Owner is Player player && player.IsRecording )
			player.AnimCapture?.AddAction( new ShootEffectsAction( IsFireContinuous ) );
	}

	public virtual void StopShootEffects()
	{
		if ( Owner is Player player && player.IsRecording )
		{
			player.AnimCapture?.AddAction( new StopAction( ShootEffectsAction.ID ) );
		}
	}
}
