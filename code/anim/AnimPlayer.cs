#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ClockBlockers.Anim;

/// <summary>
/// Responsible for playing an animation back on a pawn.
/// </summary>
public class AnimPlayer
{
	/// <summary>
	/// The pawn to play the animation on.
	/// </summary>
	public Pawn Pawn { get; }

	/// <summary>
	/// The animation to play.
	/// </summary>
	public Animation Animation { get; }

	/// <summary>
	/// Whether we're currently playing the animation
	/// </summary>
	public bool IsPlaying { get; protected set; }

	private TimeSince _timestamp = 0;

	/// <summary>
	/// The current timestamp of the animation.
	/// </summary>
	public TimeSince Timestamp { get => IsPlaying ? _timestamp : 0; private set => _timestamp = value; }

	private int _localTick = 0;
	private int _lastCurrentSegment;

	public AnimPlayer( Pawn pawn, Animation animation )
	{
		Pawn = pawn;
		Animation = animation;

		if ( Animation.Segments.Count <= 0 )
		{
			throw new ArgumentException( "Supplied animation is empty." );
		}
	}

	/// <summary>
	/// Start (or restart) animation playback.
	/// </summary>
	public void Start()
	{
		if (Pawn.ControlMethod != Pawn.PawnControlMethod.Animated)
		{
			Log.Warning( $"Pawn {Pawn} was not set to PawnControlMethod.Animated before animation playback." );
			Pawn.ControlMethod = Pawn.PawnControlMethod.Animated;
		}

		IsPlaying = true;
		Timestamp = 0;
	}

	public void Tick()
	{
		if ( !IsPlaying ) return;

		int currentSegment = Animation.TimestampToSegment( Timestamp );
		if ( currentSegment >= Animation.Segments.Count )
		{
			// The animation is over
			Stop();
			return;
		}

		if ( currentSegment != _lastCurrentSegment )
		{
			_localTick = 0;
			_lastCurrentSegment = currentSegment;
		}

		var frame = Animation.Segments[currentSegment].GetFrame( _localTick );
		frame.ApplyTo( Pawn );

		_localTick++;
	}

	public void Stop()
	{
		IsPlaying = false;
	}

	/// <summary>
	/// Create a pawn and animation player for a given animation
	/// </summary>
	/// <param name="animation">The animation to use</param>
	/// <returns>The animation player (the pawn can be extracted from this)</returns>
	public static AnimPlayer Create( Animation animation )
	{
		Pawn pawn = new();
		pawn.Clothing = animation.Clothing;
		pawn.ControlMethod = Pawn.PawnControlMethod.Animated;

		AnimPlayer player = new AnimPlayer( pawn, animation );

		AnimFrame firstFrame = animation.Segments[0].Frames[0];
		firstFrame.ApplyTo( pawn );

		return player;
	}
}
