#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using ClockBlockers.AI;

namespace ClockBlockers;

public partial class AgentPawn
{
	public AIController? AIController { get; private set; }

	[GameEvent.Tick.Server]
	public void TickAI()
	{
		Game.AssertServer();
		if ( LifeState != LifeState.Alive ) return;

		if ( AIController == null)
		{
			AIController = new AIController();
			Components.Add( AIController );
		}

		if ( ControlMethod != PawnControlMethod.AI ) return;
		AIController.Tick();
		Controller?.Simulate();
	}
}

