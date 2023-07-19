using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClockBlockers.Timeline;
using Sandbox;

namespace ClockBlockers;

public partial class AgentPawn
{
	[GameEvent.Tick.Client]
	public void ShowDebugText()
	{
		if ( !IsLocalPawn ) return;

		var debugText = $"Health: {Health}";
		if ( ActiveWeapon != null )
			debugText += $"\n Active weapon: {ActiveWeapon.GetPersistentID()} ({ActiveWeapon.ClassName})";

		DebugOverlay.ScreenText( debugText );
	}
}
