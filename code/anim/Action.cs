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

	public static IAction Shoot( Vector3 target )
	{
		return new ShootAction( target );
	}
}

class JumpAction : IAction
{
	public void Run( Pawn pawn )
	{
		pawn.DoJumpAnimation();
	}

}

class ShootAction : IAction
{
	private Vector3 _target;

	public ShootAction( Vector3 target )
	{
		this._target = target;
	}
	public void Run( Pawn pawn )
	{

	}
}
