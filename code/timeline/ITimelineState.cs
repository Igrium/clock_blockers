#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

/// <summary>
/// An entity that has a given 
/// </summary>
public interface IHasTimelineState
{
	public int GetState( AgentPawn pawn );

	/// <summary>
	/// If true, use events on entities that implent this interface
	/// will consiter the timeline state to be canon for <c>+use</c>. Therefore,
	/// they will cause unlinks if a remnant tries to use them in
	/// the wrong state.
	/// </summary>
	/// <param name="pawn">The pawn trying to use.</param>
	/// <returns></returns>
	public bool RequireUseStateMatch( AgentPawn? pawn = null )
	{
		return false;
	}
}
