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
public class AnimPlayer : EntityComponent<Player>, ISingletonComponent
{
	public AnimPlayer()
	{
		ShouldTransmit = false;
	}

	private Animation _animation = new Animation();

	private Dictionary<string, IAction> _persistentActions = new();

	/// <summary>
	/// All the persistent actions that are currently active.
	/// </summary>
	public IReadOnlyDictionary<string, IAction> PersistentActions { get => _persistentActions.AsReadOnly(); }

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
		if ( Entity.ControlMethod != AgentControlMethod.PLAYBACK )
		{
			Log.Warning( $"Pawn {Entity} was not set to AgentControlMethod.PLAYBACK before animation playback." );
			Entity.SetControlMethod( AgentControlMethod.PLAYBACK );
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
			RunAction( action );
		}

		_localTick++;
	}

	public void RunAction( IAction action )
	{
		if ( action is StopAction stop )
		{
			StopAction( stop.TargetID );
			return;
		}

		string? id = action.ActionID;
		if ( id != null )
		{
			StopAction( id );
		}
		if ( action.Run( Entity ) && id != null )
		{
			_persistentActions.Add( id, action );
		}
	}

	/// <summary>
	/// Stop an action if it is running.
	/// </summary>
	/// <param name="actionID">The action's ID</param>
	public void StopAction( string actionID )
	{
		IAction? action;
		if ( _persistentActions.TryGetValue( actionID, out action ) )
		{
			action.Stop( Entity );
			_persistentActions.Remove( actionID );
		}
	}

	/// <summary>
	/// Stop the current animation.
	/// </summary>
	/// <param name="clearActions">Stop all the persistent actions. Should only be false if a future animation intends to stop them.</param>
	public void Stop( bool clearActions = true )
	{
		IsPlaying = false;
		if ( clearActions )
		{
			foreach ( var action in _persistentActions )
			{
				action.Value.Stop( Entity );
			}
			_persistentActions.Clear();
		}

	}
}
