#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClockBlockers.Timeline;
using Sandbox;

namespace ClockBlockers.Anim;

/// <summary>
/// A discrete action performed in an animation, such as jumping or shooting.
/// </summary>
public interface IAction
{
	/// <summary>
	/// The action ID to use if this action is continuous.
	/// </summary>
	public string? ActionID => null;

	/// <summary>
	/// Run this action.
	/// </summary>
	/// <param name="pawn">Pawn to use.</param>
	/// <returns>If this action should act as a continuous action.</returns>
	public bool Run( Player pawn );

	/// <summary>
	/// Stop this action. Called before any replacement action is started.
	/// </summary>
	/// <param name="pawn">The pawn stopping the action</param>
	public void Stop( Player pawn ) {}

}

/// <summary>
/// Stops any action with the target ID.
/// Implementation is in the anim player rather than the action.
/// </summary>
public struct StopAction : IAction
{
	public string TargetID { get; set; }

	public StopAction(string targetID)
	{
		TargetID = targetID;
	}

	public bool Run( Player pawn ) { return false; }
}

public struct JumpAction : IAction
{
	public bool Run( Player pawn )
	{
		pawn.MovementController.AddEvent( "jump" );
		return false;
	}

}

public struct UseAction : IAction
{
	public static readonly string ID = "use";

	public string ActionID => ID;
	public string TargetID { get; set; } = "";
	public bool Continuous { get; set; }
	public UseAction() { }
	public UseAction(Entity target)
	{
		TargetID = target.GetPersistentIDOrThrow( true );
	}

	public bool Run( Player pawn )
	{
		Entity? target = PersistentEntities.GetEntity( TargetID );
		if (target is not IUse)
		{
			Log.Warning( $"Use target '{TargetID}' not usable!" );
			return false;
		}
		pawn.UseComponent.StartUsing( target );
		return Continuous;
	}

	public void Stop( Player pawn )
	{
		pawn.UseComponent.StopUsing();
	}
}

public struct DropWeaponAction : IAction
{
	public Vector3 Velocity { get; set; }


	public bool Run( Player pawn )
	{
		pawn.Inventory.DropItem( pawn.Inventory.ActiveChild );
		return false;
	}
}

//public struct PickUpWeaponAction : IAction
//{
//	public string WeaponID { get; set; }

//	public PickUpWeaponAction(string id)
//	{
//		WeaponID = id;
//	}

//	public PickUpWeaponAction(LegacyWeapon weapon)
//	{
//		WeaponID = weapon.GetPersistentIDOrThrow( true );
//	}

//	public void Run( AgentPawn pawn )
//	{
//		var weapon = PersistentEntities.GetEntity<LegacyWeapon>( WeaponID );
//		if ( weapon == null || weapon.IsHeld ) return;

//		pawn.PickUpWeapon( weapon );
//	}
//}
