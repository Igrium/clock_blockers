using ClockBlockers.Timeline;
using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Level;

[Library( "logic_timeline_state" )]
[HammerEntity]
[VisGroup( VisGroup.Logic )]
[EditorSprite( "editor/logic_branch.vmat" )]
[Title( "Timeline State" ), Category( "Gameplay" ), Icon( "calculate" )]
public partial class LogicTimelineState : Entity, IHasTimelineState
{
	/// <summary>
	/// The state of this trigger. An unlink will be triggered if a
	/// remnant crosses this trigger and the state is different than its canon.
	/// </summary>
	[Property( Title = "State" )]
	public int State { get; set; }

	public int GetState( AgentPawn pawn )
	{
		return State;
	}

	/// <summary>
	/// Set the state of this entity.
	/// </summary>
	/// <param name="state">The new state.</param>
	[Input]
	public void SetState(int state)
	{
		State = state;
	}
}
