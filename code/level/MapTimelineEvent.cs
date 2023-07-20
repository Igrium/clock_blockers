#nullable enable

using ClockBlockers.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ClockBlockers.Level;

public struct MapTimelineEvent : ITimelineEvent
{
	public string TriggerID { get; set; }
	public int DesiredState { get; set; }

	public string Name { get; set; }

	public bool IsValid( AgentPawn pawn )
	{
		var ent = PersistentEntities.GetEntity<Entity>( TriggerID );
		if ( ent is IHasTimelineState trigger )
		{
			return trigger.GetState( pawn ) == DesiredState;
		}
		else
		{
			return false;
		}
	}
}
