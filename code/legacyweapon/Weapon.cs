#nullable enable

using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

/// <summary>
/// A weapon with a world model and a view model that can be held by the player.
/// </summary>
public abstract partial class LegacyWeapon : AnimatedEntity, IUse, IUseNotCanon
{

	/// <summary>
	/// The weapon's viewmodel entity. Only available client-side.
	/// </summary>
	public WeaponViewModel? ViewModel { get; protected set; }

	/// <summary>
	/// The owning pawn.
	/// </summary>
	public AgentPawn? Pawn => Owner as AgentPawn;

	/// <summary>
	/// This'll decide which entity to fire effects from. If we're in first person, the View Model, otherwise, this.
	/// </summary>
	public AnimatedEntity EffectEntity => (ViewModel != null && Camera.FirstPersonViewer == Owner) ? ViewModel : this;

	/// <summary>
	/// Path to the weapon's viewmodel
	/// </summary>
	public abstract string ViewModelPath { get; }

	/// <summary>
	/// Path to the wepon's world model
	/// </summary>
	public abstract string WorldModelPath { get; }

	/// <summary>
	/// Whether this weapon can be dropped on the ground.
	/// Make sure the world model has physics if this is true.
	/// </summary>
	public virtual bool CanDrop => true;

	/// <summary>
	/// If this weapon is being held by an agent.
	/// </summary>
	public bool IsHeld => Owner != null;

	public abstract CitizenAnimationHelper.HoldTypes HoldType { get; }

	public override void Spawn()
	{
		Tags.Add( "weapon" );
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetModel( WorldModelPath );
		EnablePhysics();

	}

	/// <summary>
	/// Called every tick on the server and the owning client when this weapon is active.
	/// </summary>
	public virtual void Tick()
	{
		Animate();
	}

	/// <summary>
	/// Called when <see cref="AgentPawn.SetActiveWeapon(LegacyWeapon)"/> is called for this weapon.
	/// </summary>
	/// <param name="pawn">The pawn</param>
	public virtual void OnEquip( AgentPawn pawn )
	{
		DisablePhysics();

		Owner = pawn;
		SetParent( pawn, true );
		EnableDrawing = true;
		CreateViewModel( To.Single( pawn ) );
	}

	/// <summary>
	/// Called when the weapon is either removed from the player, or holstered.
	/// </summary>
	public virtual void OnHolster()
	{
		EnableDrawing = false;
		DestroyViewModel( To.Single( Owner ) );
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );
		ViewModel = vm;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModel != null && ViewModel.IsValid )
		{
			ViewModel.Delete();
		}
	}

	/// <summary>
	/// Called every tick to animate this weapon.
	/// </summary>
	public virtual void Animate()
	{
		Pawn?.SetAnimParameter( "holdtype", (int)HoldType );
	}

	public virtual void OnDrop()
	{
		if ( !CanDrop )
		{
			throw new InvalidOperationException( "This weapon can't be dropped." );
		}

		var vel = Velocity;
		var pos = Position;

		OnHolster();
		SetParent( null );
		Owner = null;


		EnableDrawing = true;
		EnablePhysics();

		Position = pos;
		Velocity = vel;
	}

	protected void EnablePhysics()
	{
		PhysicsEnabled = true;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		EnableSolidCollisions = true;
	}

	protected void DisablePhysics()
	{
		PhysicsEnabled = false;
		EnableSolidCollisions = false;
		PhysicsClear();
	}

	public virtual bool OnUse( Entity user )
	{
		if ( user is not AgentPawn pawn ) return false;

		pawn.PickUpWeapon( this );
		return true;
	}

	public virtual bool IsUsable( Entity user )
	{
		if ( IsHeld ) return false;
		if (user is AgentPawn pawn && pawn.ActiveWeapon != null && !pawn.ActiveWeapon.CanDrop )
		{
			return false;
		}
		return true;
	}
}
