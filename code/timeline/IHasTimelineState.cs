#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

public interface IHasTimelineState
{
	public int GetState( Pawn? pawn );

	/// <summary>
	/// If true, use events on entities that implent this interface
	/// will consiter the timeline state to be canon. Therefore,
	/// they will cause unlinks if a remnant tries to use them in
	/// the wrong state.
	/// </summary>
	/// <param name="pawn">The pawn trying to use.</param>
	/// <returns></returns>
	public bool RequireUseStateMatch( Pawn? pawn )
	{
		return false;
	}
}

