using ClockBlockers.Timeline;
using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	public void Run( Pawn pawn );

	private static JumpAction _jumpInstance = new JumpAction();
	
	public static IAction Jump()
	{
		return _jumpInstance;
	}

}

struct JumpAction : IAction
{
	public void Run( Pawn pawn )
	{
		pawn.DoJumpAnimation();
	}

}


public struct UseAction : IAction
{
	public string TargetID { get; set; }

	public void Run(Pawn pawn)
	{
		var target = PersistentEntities.GetEntity<Entity>( TargetID );
		if ( target is not IUse ) return;
		pawn.Use( target );
	}
}
