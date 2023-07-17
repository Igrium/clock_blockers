#nullable enable

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

	public bool IsValid( Pawn pawn )
	{
		return pawn.LifeState != LifeState.Alive;
	}
}

public struct GameEndEvent : ITimelineEvent
{
	public string Name => "Game End";

	public bool IsValid( Pawn pawn )
	{
		return true;
	}
}

/// <summary>
/// A pawn has used +use on an entity.
/// </summary>
public class UseEvent : ITimelineEvent
{
	private string EntityName;

	public string Name => "Use " + EntityName;

	public string TargetID { get; set; }

	public Vector3 CanonPosition { get; set; }

	public int? EntityState { get; set; }

	/// <summary>
	/// Create a use event.
	/// </summary>
	/// <param name="entity">The entity to use.</param>
	/// <param name="pawn">The pawn using the entity.</param>
	/// <param name="requireStateMatch">If this entity implements <c>IHasTimelineState</c>,
	/// require the state to match in order for the event to be valid.</param>
	/// <exception cref="InvalidOperationException">If the target is not usable</exception>
	public UseEvent( Entity entity, Pawn pawn, bool requireStateMatch = false )
	{
		if ( entity is not IUse )
		{
			throw new InvalidOperationException( "Target entity must be usable." );
		}

		TargetID = entity.GetPersistentIDOrThrow( true );
		EntityName = entity.Name;
		CanonPosition = entity.Position;

		if ( requireStateMatch && entity is IHasTimelineState stateHolder )
		{
			EntityState = stateHolder.GetState( pawn );
		}
	}

	public bool IsValid( Pawn pawn )
	{
		var entity = PersistentEntities.GetEntity<Entity>( TargetID );
		if ( entity is not IUse use || !use.IsUsable(pawn) ) return false;

		if ( entity is IHasTimelineState stateHolder && EntityState.HasValue )
		{
			if ( stateHolder.GetState( pawn ) != EntityState.Value ) return false;
		}

		// This entity has moved too far from its canon position.
		if ( entity.Position.Distance( CanonPosition ) > 64 ) return false;

		return true;
	}
}
