#nullable enable

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
public abstract partial class Weapon : AnimatedEntity
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

	public abstract CitizenAnimationHelper.HoldTypes HoldType { get; }

	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		SetModel( WorldModelPath );
	}

	/// <summary>
	/// Called every tick on the server and the owning client when this weapon is active.
	/// </summary>
	public virtual void Tick()
	{
		Animate();
	}

	/// <summary>
	/// Called when <see cref="AgentPawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
	/// </summary>
	/// <param name="pawn">The pawn</param>
	public virtual void OnEquip( AgentPawn pawn )
	{
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
}
