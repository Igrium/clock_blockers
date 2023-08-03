using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Level;

/// <summary>
/// Displays text on the screen when triggered. Useful for testing IO.
/// </summary>
[Library( "debug_overlay" )]
[HammerEntity]
[VisGroup( VisGroup.Logic )]
[Title( "Logic Toggle" ), Category( "Gameplay" ), Icon( "calculate" )]
public partial class DebugOverlayEntity : Entity
{
	[Property(Title = "Duration")]
	public float Duration { get; set; } = 3f;

	[Input]
	public void Trigger( Entity activator, string? text = null )
	{
		if ( text == null )
			text = $"{this} has been triggered.";
		ShowText( text, Duration );
	}

	[ClientRpc]
	public void ShowText(string text, float time)
	{
		DebugOverlay.ScreenText( text, duration: time );
	}
}
