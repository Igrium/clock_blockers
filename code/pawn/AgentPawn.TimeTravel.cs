#nullable enable

using ClockBlockers.Anim;
using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

/*
 * All the timey-wimey stuff regarding agents
 */

public partial class AgentPawn
{
	public AnimPlayer AnimPlayer { get; protected set; } = new AnimPlayer();
	public TimelinePlayer? TimelinePlayer { get; protected set; }

	public AnimCapture? AnimCapture { get; protected set; }

	public TimelineCapture? TimelineCapture { get; protected set; }

	public bool IsRecording => AnimCapture != null && AnimCapture.IsRecording;

	/// <summary>
	/// The current place in this entity's branch tree. Used for testing.
	/// </summary>
	[Net]
	public IList<bool> Branches { get; protected set; } = new List<bool>();

	public TimelineBranch? ActiveTimeline
	{
		get
		{
			if ( TimelineCapture != null )
			{
				return TimelineCapture.RootBranch;
			}
			else if ( TimelinePlayer != null )
			{
				return TimelinePlayer.RootBranch;
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Initialize all the timey-wimey code for this pawn.
	/// </summary>
	/// <param name="controlMethod">The control method to use.</param>
	/// <param name="branch">If the control method is <c>ANIMATED</c>, the timeline to play.</param>
	/// <exception cref="ArgumentException">If you don't specify a timeline with an animated control method.</exception>
	public void InitTimeTravel( PawnControlMethod controlMethod, TimelineBranch? branch = null )
	{
		ControlMethod = controlMethod;
		if ( IsFreeAgent )
		{
			TimelineCapture = Components.Create<TimelineCapture>();
			TimelineCapture.StartCapture();
		}
		else
		{
			if ( branch == null )
			{
				throw new ArgumentException( "Timeline branch must be specified when using animated control method.", "branch" );
			}

			TimelinePlayer = Components.GetOrCreate<TimelinePlayer>();
			TimelinePlayer.PlayTimeline( branch, root: true );
		}
	}

	public void FinalizeAnimations()
	{
		TimelineCapture?.Complete();
		TimelinePlayer?.Stop();

		if ( AnimCapture != null && AnimCapture.IsRecording )
		{
			AnimCapture.Stop();
		}
	}

	public void PlayAnimation( Animation animation )
	{
		AnimPlayer.Play( animation );
	}

	public void StartCapture()
	{
		if ( AnimCapture != null )
		{
			Log.Warning( $"Entity {this} is already capturing. Discarding existing capture." );
			AnimCapture.Stop();
		}

		AnimCapture = new AnimCapture();
		Components.Add( AnimCapture );
		AnimCapture.Start();
	}

	public Animation StopCapture()
	{
		if ( AnimCapture == null )
		{
			throw new InvalidOperationException( "Pawn is not capturing." );
		}
		var anim = AnimCapture.Stop();
		AnimCapture = null;
		return anim;
	}

	/// <summary>
	/// Called when the pawn is being unlinked.
	/// This is only called when the pawn is about to become a free agent
	/// </summary>
	/// <param name="branch">The branch that was being played when the unlink happened</param>
	/// <param name="timelinePlayer">The active timeline player.</param>
	public void OnUnlink( TimelineBranch branch, TimelinePlayer timelinePlayer )
	{
		if ( ControlMethod != PawnControlMethod.Animated )
		{
			throw new InvalidOperationException( "Unlink was called on a free agent." );
		}
		timelinePlayer.Stop();

		// When we unlink, we switch from PLAYING a timeline to CAPTURING a timeline.
		TimelineCapture = Components.Create<TimelineCapture>();
		TimelineCapture.ForkedBranch = branch;

		TimelineCapture.StartCapture();

		ControlMethod = PawnControlMethod.AI;

		Log.Info( $"{this.GetPersistentID()} has unlinked!" );
	}

}
