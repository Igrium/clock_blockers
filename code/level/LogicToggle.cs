using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Level;

/// <summary>
/// Facepunch removed this for some reason which makes no sense.
/// I've reimplemented it for now until I can find a better solution.
/// </summary>
[Library( "logic_toggle" )]
[HammerEntity]
[VisGroup( VisGroup.Logic )]
[EditorSprite( "editor/ent_logic.vmat" )]
[Title( "Logic Toggle" ), Category( "Gameplay" ), Icon( "calculate" )]
public partial class LogicToggle : Entity
{
	[Property( Title = "Default State" )]
	public static bool State { get; set; }

	[Input]
	public void SetState( bool state )
	{
		State = state;
	}

	[Input]
	public void Trigger( Entity activator )
	{
		if ( State )
		{
			OnTrue.Fire( activator );
		}
		else
		{
			OnFalse.Fire( activator );
		}
	}

	[Input]
	public void Toggle( Entity activator )
	{
		State = !State;
		Trigger( activator );
	}

	public Output OnTrue { get; set; }
	public Output OnFalse { get; set; }
}
