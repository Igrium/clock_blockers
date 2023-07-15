#nullable enable

using ClockBlockers.Anim;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

/// <summary>
/// Records a series of events within a single timeline.
/// There is no branching here because this is only for free agents.
/// </summary>
public partial class TimelineCapture : EntityComponent<Pawn>, ISingletonComponent
{
	public TimelineCapture()
	{
		ShouldTransmit = false;
	}

	public TimelineBranch? RootBranch { get; set; }
	public TimelineBranch? ForkedBranch { get; set; }

	private TimelineBranch? prevBranch;

	public void StartCapture()
	{
		Entity.StartCapture();
	}

	public void Event( ITimelineEvent timelineEvent, bool final = false )
	{
		var id = Entity.GetPersistentID();
		if ( id == null )
		{
			Log.Warning( $"Entity: {Entity} does not have a persistent ID. This may cause issues when playing back timeline." );
		}

		var anim = Entity.StopCapture();
		var branch = new TimelineBranch( anim )
		{
			EndEvent = timelineEvent,
			EndEventTime = anim.Length,
			PersistentID = id
		};

		if ( RootBranch == null )
		{
			RootBranch = branch;
		}
		else if ( prevBranch == null )
		{
			// The root branch may have been set manually before capture.
			prevBranch = RootBranch;
		}

		if ( prevBranch != null )
		{
			// Add to the end of the previous branch
			prevBranch.BranchA = branch;
		}

		if ( ForkedBranch != null )
		{
			ForkedBranch.BranchB = branch;
		}

		prevBranch = branch;

		if ( !final )
		{
			Entity.StartCapture();
		}
	}

	/// <summary>
	/// Called at the end of the round to complete recording.
	/// </summary>
	/// <returns>The root timeline.</returns>
	public TimelineBranch Complete()
	{
		if ( Entity.IsRecording )
		{
			Event( new GameEndEvent(), final: true );
		}

		if ( RootBranch == null )
			throw new InvalidOperationException( "The timeline completed with no branches. This should not happen." );

		return RootBranch;
	}
}
