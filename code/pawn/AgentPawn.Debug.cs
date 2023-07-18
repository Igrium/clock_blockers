using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ClockBlockers;

public partial class AgentPawn
{
	[GameEvent.Tick.Client]
	public void ShowDebugText()
	{
		if (IsLocalPawn) DebugOverlay.ScreenText( $"Health: {Health}" );
	}
}
