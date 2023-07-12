#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

public partial class TimelinePlayer : EntityComponent<Pawn>, ISingletonComponent
{
	public TimelineBranch? Branch { get; private set; }

	public bool IsPlaying => Branch != null;

	// Store this seperately so we can access it after the animaiton stops.
	private TimeSince timePlaying = 0;

	/// <summary>
	/// Play a timeline.
	/// </summary>
	/// <param name="rootBranch">The root timeline branch.</param>
	public void PlayTimeline( TimelineBranch rootBranch )
	{
		Entity.AnimPlayer.Play( rootBranch.Animation );
		Branch = rootBranch;
		timePlaying = 0;
	}

	public void Tick()
	{
		if ( Branch == null ) return;

		// Without an end event, we'll just stop here.
		if ( Branch.EndEventTime <= timePlaying && Branch.EndEvent != null )
		{
			tryPlayBranch( Branch.EndEvent.IsValid( Entity ) ? Branch.BranchA : Branch.BranchB );
		}
	}

	/// <summary>
	/// Stops the timeline player.
	/// The current animation will finish playing, but it will stop at the next event.
	/// </summary>
	public void Stop()
	{
		if ( !IsPlaying ) return;
		Branch = null;
	}

	private void tryPlayBranch( TimelineBranch? branch )
	{
		if ( branch != null )
		{
			PlayTimeline( branch );
		}
		else
		{
			Unlink();
		}
	}

	// TODO: implement unlinking
	public void Unlink()
	{

	}

}
