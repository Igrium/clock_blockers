#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using ClockBlockers.AI;

namespace ClockBlockers;

public partial class Pawn
{
	private AIController? _aiController;

	public AIController AIController
	{
		get
		{
			if ( _aiController != null ) return _aiController;
			else
			{
				_aiController = Components.Create<AIController>();
				return _aiController;
			}
		}
	}

	[GameEvent.Tick.Server]
	public void TickAI()
	{
		Game.AssertServer();
		if ( ControlMethod != PawnControlMethod.AI ) return;
		AIController.Tick();
		Controller?.Simulate();
	}
}

