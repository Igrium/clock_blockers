#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Anim;

static class IListExtension
{
	public static void AddRange<T>( this IList<T> list, IEnumerable<T> items )
	{
		if ( list == null ) throw new ArgumentNullException( nameof( list ) );
		if ( items == null ) throw new ArgumentNullException( nameof( items ) );

		if ( list is List<T> asList )
		{
			asList.AddRange( items );
		}
		else
		{
			foreach ( var item in items )
			{
				list.Add( item );
			}
		}
	}
}

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

	public IDictionary<int, IList<IAction>> Actions { get; } = new Dictionary<int, IList<IAction>>();

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

	/// <summary>
	/// Return an enumerable of all the actions in a given tick.
	/// If there are no actions in this tick, the enumerable is empty.
	/// </summary>
	/// <param name="tickIndex">The tick index.</param>
	/// <returns>The actions.</returns>
	public IEnumerable<IAction> GetActions( int tickIndex )
	{
		IList<IAction>? tickActions;
		if ( Actions.TryGetValue( tickIndex, out tickActions ) )
		{
			foreach( var action in tickActions )
			{
				yield return action;
			}
		}

		yield break;
	}

	/// <summary>
	/// Add an action to a given tick.
	/// </summary>
	/// <param name="tickIndex">Tick index to add to.</param>
	/// <param name="action">Action to add.</param>
	public void AddAction( int tickIndex, IAction action )
	{
		_getOrCreate( tickIndex ).Add( action );
	}

	/// <summary>
	/// Add a sequence of actions to a given tick.
	/// </summary>
	/// <param name="tickIndex">Tick index to add to.</param>
	/// <param name="actions">Actions to add.</param>
	public void AddActions( int tickIndex, IEnumerable<IAction> actions)
	{
		if ( actions.Count() == 0 ) return;
		_getOrCreate( tickIndex ).AddRange( actions );
	}

	private IList<IAction> _getOrCreate(int tickIndex)
	{
		IList<IAction>? tickActions;
		if ( !Actions.TryGetValue( tickIndex, out tickActions ) )
		{
			tickActions = new List<IAction>();
			Actions.Add( tickIndex, tickActions );
		}
		return tickActions;
	}
}

/// <summary>
/// A single frame of animation
/// </summary>
public struct AnimFrame
{
	public Vector3 Position { get; set; }
	public Vector3 Velocity { get; set; }
	//public Rotation EyeRotation { get; set; }
	public Angles ViewAngles { get; set; }
	public Rotation Rotation { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsDucked { get; set; }

	/// <summary>
	/// Capture a frame from a pawn's current state.
	/// </summary>
	/// <param name="pawn">Pawn to capture.</param>
	/// <returns>The frame.</returns>
	public static AnimFrame Capture( PlayerAgent pawn )
	{
		return new()
		{
			Position = pawn.Position,
			Velocity = pawn.Velocity,
			Rotation = pawn.Rotation,
			ViewAngles = pawn.ViewAngles,
			IsGrounded = pawn.IsGrounded,
			IsDucked = pawn.MovementController.HasTag( "ducked" )
		};
	}

	/// <summary>
	/// Apply this frame to a pawn.
	/// </summary>
	/// <param name="pawn">Pawn to apply to.</param>
	public void ApplyTo( PlayerAgent pawn )
	{
		pawn.Position = Position;
		pawn.Velocity = Velocity;
		pawn.Rotation = Rotation;
		pawn.ViewAngles = ViewAngles;
		pawn.IsGrounded = IsGrounded;
		if (IsDucked)
			pawn.MovementController.SetTag( "ducked" );
	}

	public override string ToString()
	{
		return $"AnimFrame[{Position}]";
	}

}
