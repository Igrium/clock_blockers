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
	/// Run this action.
	/// </summary>
	/// <param name="pawn">Pawn to use.</param>
	public void Run( AgentPawn pawn );

	private static JumpAction _jumpInstance = new JumpAction();

	public static IAction Jump()
	{
		return _jumpInstance;
	}
}

class JumpAction : IAction
{
	public void Run( AgentPawn pawn )
	{
		pawn.DoJumpAnimation();
	}

}

/// <summary>
/// The pawn uses <c>+use</c> on an entity.
/// </summary>
public struct UseAction : IAction
{
	/// <summary>
	/// The persistent ID of the target entity.
	/// </summary>
	public string TargetID { get; set; } = "";

	public UseAction() { }

	public UseAction( Entity target )
	{
		TargetID = target.GetPersistentIDOrThrow( generate: true );
	}

	public void Run( AgentPawn pawn )
	{
		var ent = PersistentEntities.GetEntity( TargetID );
		if ( ent is not IUse target || !target.IsUsable( pawn ) )
			return;

		pawn.Use( ent );
	}
}

public struct DropWeaponAction : IAction
{
	public Vector3 Velocity { get; set; }

	public void Run( AgentPawn pawn )
	{
		pawn.DropWeapon( Velocity );
	}
}

public struct PickUpWeaponAction : IAction
{
	public string WeaponID { get; set; }

	public PickUpWeaponAction(string id)
	{
		WeaponID = id;
	}

	public PickUpWeaponAction(Weapon weapon)
	{
		WeaponID = weapon.GetPersistentIDOrThrow( true );
	}

	public void Run( AgentPawn pawn )
	{
		var weapon = PersistentEntities.GetEntity<Weapon>( WeaponID );
		if ( weapon == null || weapon.IsHeld ) return;

		pawn.PickUpWeapon( weapon );
	}
}
