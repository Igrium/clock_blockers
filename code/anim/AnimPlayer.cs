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
public class AnimPlayer : EntityComponent<Pawn>, ISingletonComponent
{

	private Animation _animation = new Animation();

	/// <summary>
	/// The animation to play.
	/// </summary>
	public Animation Animation
	{
		get => _animation; set
		{
			if ( IsPlaying ) throw new InvalidOperationException( "Cannot change animation while playing." );
			_animation = value;
		}
	}

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

	/// <summary>
	/// Set an animation and play it.
	/// </summary>
	/// <param name="animation">The animation.</param>
	public void Play( Animation animation )
	{
		Stop();
		Animation = animation;
		Start();
	}

	/// <summary>
	/// Start (or restart) animation playback.
	/// </summary>
	public void Start()
	{
		if ( Entity.ControlMethod != Pawn.PawnControlMethod.Animated )
		{
			Log.Warning( $"Pawn {Entity} was not set to PawnControlMethod.Animated before animation playback." );
			Entity.ControlMethod = Pawn.PawnControlMethod.Animated;
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

		var segment = Animation.Segments[currentSegment];
		var frame = segment.GetFrame( _localTick );
		frame.ApplyTo( Entity );

		foreach ( IAction action in segment.GetActions( _localTick ) )
		{
			action.Run( Entity );
		}

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
	public static Pawn Create( Animation animation )
	{
		if ( animation.IsEmpty )
		{
			throw new ArgumentException( "Supplied animation is empty." );
		}


		Pawn pawn = new();
		pawn.PostSpawn();
		pawn.Clothing = animation.Clothing;
		pawn.ControlMethod = Pawn.PawnControlMethod.Animated;

		animation.Segments[0].Frames[0].ApplyTo( pawn );

		pawn.PlayAnimation( animation );

		return pawn;
	}
}
