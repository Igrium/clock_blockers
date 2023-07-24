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
	public bool IsValid( Player pawn );

	/// <summary>
	/// The name to show in the UI regarding this event.
	/// </summary>
	public string Name { get; }
}

/// <summary>
/// A dummy event to use when the player dies.
/// </summary>
public struct DeathEvent : ITimelineEvent
{
	public string Name => "Death";

	public bool IsValid( Player pawn )
	{
		return pawn.LifeState != LifeState.Alive;
	}
}

public struct GameEndEvent : ITimelineEvent
{
	public string Name => "Game End";

	public bool IsValid( Player pawn )
	{
		return true;
	}
}

public struct UseEvent : ITimelineEvent
{

	public string Name => $"Use {TargetID}";

	public string TargetID { get; init; }

	/// <summary>
	/// The desired timeline state for stateholders.
	/// </summary>
	public int? DesiredState { get; set; }

	public UseEvent( Entity target )
	{
		TargetID = target.GetPersistentID( generate: true );
	}

	public UseEvent( string targetID )
	{
		TargetID = targetID;
	}

	public bool IsValid( Player pawn )
	{
		var ent = PersistentEntities.GetEntity( TargetID );
		if ( ent is not IUse usable || !usable.IsUsable( pawn ) )
			return false;

		if ( ent is IHasTimelineState stateholder && stateholder.RequireUseStateMatch( pawn ) && DesiredState.HasValue )
		{
			if ( stateholder.GetState( pawn ) != DesiredState.Value ) return false;
		}

		return true;
	}
}
