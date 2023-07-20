#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Util;

public static class SpawnPointExtensions
{
	/// <summary>
	/// Check if this spawn point is occupied by an entity.
	/// </summary>
	/// <param name="spawnPoint">Spawn point to check.</param>
	/// <param name="predicate">A predicate to apply to entities. If null, will check for </param>
	/// <param name="radius">Radius from the origin to search for entities in.</param>
	/// <returns>If the spawn point is occupied.</returns>
	public static bool IsOccupied( this SpawnPoint spawnPoint, Func<Entity, bool>? predicate = null, float radius = 16 )
	{
		if ( predicate == null )
			predicate = ent => ent.Tags.Has( "player" );

		var iter = Entity.FindInSphere( spawnPoint.Position, radius ).Where( predicate );

		return iter.Any();
	}
}
