using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

/// <summary>
/// An event is what's considered "canon" on an agent's timeline. Only disruptions in an event may cause branches.
/// Events must be persistent between rounds; therefore it must reference persistent IDs instead of entities directly.
/// </summary>
public interface ITimelineEvent
{
	/// <summary>
	/// Check whether this event is "valid" in the given context. That is, the conditions are right for it to occur or it has occured.
	/// </summary>
	/// <param name="pawn">The pawn executing this event.</param>
	/// <returns>If the event is valid. If this returns false, the pawn will unlink.</returns>
	public bool IsValid( Pawn pawn );
}

/// <summary>
/// A dummy event to use when the player dies.
/// </summary>
public struct DeathEvent : ITimelineEvent
{
	public bool IsValid( Pawn pawn )
	{
		return pawn.LifeState != LifeState.Alive;
	}
}

public struct GameEndEvent : ITimelineEvent
{
	public bool IsValid( Pawn pawn )
	{
		return true;
	}
}
