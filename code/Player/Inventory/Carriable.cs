#nullable enable

using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Linq;

namespace ClockBlockers;
/// <summary>
///  Something that can go into the player's inventory and have a worldmodel and viewmodel etc, 
/// </summary>
public abstract partial class Carriable : AnimatedEntity, IUse, IUseNotCanon
{

	/// <summary>
	/// Utility - return the entity we should be spawning particles from etc
	/// </summary>
	public virtual ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;
	public abstract string? WorldModelPath { get; }
	public abstract string? ViewModelPath { get; }

	public Player? Pawn => Owner is Player player ? player : null;

	/// <summary>
	/// Whether this item is allowed to be dropped.
	/// </summary>
	public virtual bool CanDrop => false;

	/// <summary>
	/// Whether this item is currently being held by an agent.
	/// </summary>
	[Net]
	public bool IsActive { get; protected set; }

	public virtual CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Pistol;
	public virtual CitizenAnimationHelper.Hand Handedness => CitizenAnimationHelper.Hand.Both;
	public BaseViewModel? ViewModelEntity { get; protected set; }
	public override void Spawn()
	{
		base.Spawn();
		CarriableSpawn();
		Tags.Add( "weapon" );
	}
	internal virtual void CarriableSpawn()
	{
		SetModel( WorldModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		EnableTouch = true;
	}
	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
	[ClientRpc]
	public virtual void CreateViewModel()
	{
		Game.AssertClient();
		if ( Owner == null || !Owner.IsLocalPawn ) return;

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new BaseViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( ViewModelPath );
	}

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	[ClientRpc]
	public virtual void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}
	//public override void StartTouch( Entity other )
	//{
	//	base.Touch( other );
	//	if ( other is Player ply )
	//	{
	//		if ( ply.Inventory?.Items.Where( x => x.GetType() == this.GetType() ).Count() <= 0 )
	//		{

	//			ply.Inventory?.AddItem( this );
	//		}
	//		else
	//		{
	//			//if ( this is Weapon wep )
	//			//{
	//			//	wep.PrimaryAmmo -= (int)(ply.Ammo?.GiveAmmo( wep.PrimaryAmmoType, wep.PrimaryAmmo ));
	//			//	wep.SecondaryAmmo -= (int)(ply.Ammo?.GiveAmmo( wep.SecondaryAmmoType, wep.SecondaryAmmo ));
	//			//	if ( wep.PrimaryAmmo <= 0 && wep.SecondaryAmmo <= 0 )
	//			//	{
	//			//		wep.Delete();
	//			//	}
	//			//}
	//		}
	//	}
	//}
	public virtual void OnPickup( Entity equipper )
	{
		SetParent( equipper, true );
		Owner = equipper;
		PhysicsEnabled = false;
		EnableAllCollisions = false;
		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}
	public virtual void OnDrop( Entity dropper )
	{
		var vel = Velocity;
		var pos = Position;

		SetParent( null );
		Owner = null;
		PhysicsEnabled = true;
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = false;
		EnableShadowInFirstPerson = false;
		OnActiveEnd();

		Position = pos;
		Velocity = vel;
	}
	public virtual void OnActiveStart()
	{
		EnableDrawing = true;
		DestroyViewModel();
		CreateViewModel();
		IsActive = true;
	}
	public virtual void OnActiveEnd()
	{
		if ( Parent is Player ) EnableDrawing = false;
		DestroyViewModel();
		IsActive = false;
	}
	public virtual void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = HoldType;
		anim.Handedness = Handedness;
		anim.AimBodyWeight = 1.0f;
	}

	public bool OnUse( Entity user )
	{
		if ( user is not Player player )
			return false;

		player.Inventory.PickupItem( this );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return (user is Player && Parent == null);
	}
}
