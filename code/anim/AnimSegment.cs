#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Anim;

/// <summary>
/// A one second long segment of animation. 
/// Anims are broken into segments that are based on time rather than ticks to account for tick drops.
/// </summary>
public class AnimSegment
{

	/// <summary>
	/// All the frames in this segment; one frame per tick.
	/// </summary>
	public IList<AnimFrame> Frames { get; protected set; } = new List<AnimFrame>();

	/// <summary>
	/// The number of frames in this segment. Should generally be equal to the tickrate.
	/// </summary>
	public int Count { get => Frames.Count; }

	public AnimFrame GetFrame( int tickIndex )
	{
		if ( Frames.Count == 0 )
		{
			throw new InvalidOperationException( "Anim segment is empty!" );
		}

		if ( tickIndex < 0 )
		{
			tickIndex = 0;
		}
		if ( tickIndex >= Frames.Count )
		{
			tickIndex = Frames.Count - 1;
		}
		return Frames[tickIndex];
	}
}

/// <summary>
/// A single frame of animation
/// </summary>
public struct AnimFrame
{
	public Vector3 Position { get; set; }
	public Vector3 Velocity { get; set; }
	public Rotation EyeRotation { get; set; }
	public Rotation Rotation { get; set; }
	public bool IsGrounded { get; set; }
	public bool DidJump { get; set; }

	/// <summary>
	/// Capture a frame from a pawn's current state.
	/// </summary>
	/// <param name="pawn">Pawn to capture.</param>
	/// <returns>The frame.</returns>
	public static AnimFrame Capture( Pawn pawn )
	{
		return new()
		{
			Position = pawn.Position,
			Velocity = pawn.Velocity,
			Rotation = pawn.Rotation,
			EyeRotation = pawn.EyeRotation,
			IsGrounded = pawn.IsGrounded,
			DidJump = pawn.DidJump
		};
	}

	/// <summary>
	/// Apply this frame to a pawn.
	/// </summary>
	/// <param name="pawn">Pawn to apply to.</param>
	public void ApplyTo( Pawn pawn )
	{
		pawn.Position = Position;
		pawn.Velocity = Velocity;
		pawn.Rotation = Rotation;
		pawn.EyeRotation = EyeRotation;
		pawn.IsGrounded = IsGrounded;
		pawn.DidJump = DidJump;
	}

	public override string ToString()
	{
		return $"AnimFrame[{Position}]";
	}

}
