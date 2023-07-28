using ClockBlockers.Timeline;
using ClockBlockers.Weapon;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Anim;


public struct ShootAction : IAction
{
	public BulletInfo Bullet { get; init; }

	/// <summary>
	/// The lag-compensated positions of all relevent entities.
	/// </summary>
	public Dictionary<string, EntityTraceState> EntityPositions { get; set; } = new();

	public ShootAction( BulletInfo bullet )
	{
		Bullet = bullet;
	}

	public bool Run( PlayerAgent pawn )
	{
		if ( pawn.ActiveWeapon is not BaseFirearm firearm ) return false;

		// Temporarily restore all entities to recorded positions
		Dictionary<ModelEntity, EntityTraceState> originalPositions = new();
		foreach ( var entry in EntityPositions )
		{
			var target = PersistentEntities.GetEntity<ModelEntity>( entry.Key );
			if ( target != null )
			{
				Log.Info( $"Original Position: {target.Position}; Lag compensated: {entry.Value}" );
				originalPositions.Add( target, new EntityTraceState().CopyFrom( target ) );
				//originalPositions.Add( target, target.Position );
				entry.Value.ApplyTo( target );
				
			}
		}

		firearm.FireBullet( Bullet, true );

		foreach ( var entry in originalPositions )
		{
			entry.Value.ApplyTo( entry.Key );
		}

		return false;
	}
}

public struct EntityTraceState
{
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }

	public Transform[] BoneTransforms { get; set; } = new Transform[0];

	public EntityTraceState()
	{ }

	public EntityTraceState CopyFrom( ModelEntity entity )
	{
		Position = entity.Position;
		Rotation = entity.Rotation;
		BoneTransforms = new Transform[entity.BoneCount];

		for ( int i = 0; i < BoneTransforms.Length; i++ )
		{
			BoneTransforms[i] = entity.GetBoneTransform( i );
		}
		return this;
	}

	public void ApplyTo( ModelEntity entity )
	{
		entity.Position = Position;
		entity.Rotation = Rotation;

		for ( int i = 0; i < BoneTransforms.Length; i++ )
		{
			entity.SetBoneTransform( i, BoneTransforms[i] );
		}
		
	}
}
