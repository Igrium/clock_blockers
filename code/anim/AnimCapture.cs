#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Anim;

/// <summary>
/// Responsible for capturing the animation of a pawn.
/// </summary>
public class AnimCapture
{
	private TimeSince _startTime;

	/// <summary>
	/// Whether this anim capture is currently recording.
	/// </summary>
	public bool IsRecording { get; private set; }

	/// <summary>
	/// The animation this capture is recording (or recorded) to.
	/// </summary>
	public Animation? Animation { get; private set; }

	/// <summary>
	/// The pawn to be recorded.
	/// </summary>
	public Pawn Pawn { get; private set; }

	/// <summary>
	/// Create an anim capture.
	/// </summary>
	/// <param name="pawn">The pawn to be recorded.</param>
	public AnimCapture( Pawn pawn )
	{
		Pawn = pawn;
	}

	public void Start()
	{
		if ( Animation != null )
		{
			throw new InvalidOperationException( "Animation already is recording or has finished." );
		}

		Animation = new();
		Animation.TickRate = Game.TickRate;
		Animation.Clothing = Pawn.Clothing;

		IsRecording = true;
		_startTime = 0;
	}


	public void Tick()
	{
		if ( !IsRecording || Animation == null ) return;

		int segmentIndex = (int)MathF.Floor( _startTime );

		// Generally this should act as an if statement, only iterating once.
		while ( segmentIndex >= Animation.Segments.Count )
		{
			Animation.Segments.Add( new AnimSegment() );
		}

		AnimSegment segment = Animation.Segments[segmentIndex];

		segment.Frames.Add( AnimFrame.Capture( Pawn ) );
		
	}

	public Animation Stop()
	{
		if ( Animation == null )
		{
			throw new InvalidOperationException( "Not recording." );
		};

		IsRecording = false;
		return Animation;
	}
}
