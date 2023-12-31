﻿#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

public partial class TimelinePlayer : EntityComponent<PlayerAgent>, ISingletonComponent
{
	/// <summary>
	/// The branch currently being played
	/// </summary>
	public TimelineBranch? Branch { get; private set; }

	/// <summary>
	/// The root branch in this timeline tree. Only used as metadata.
	/// </summary>
	public TimelineBranch? RootBranch { get; set; }

	public bool IsPlaying => Branch != null;

	// Store this seperately so we can access it after the animaiton stops.
	private TimeSince timePlaying = 0;

	public TimelinePlayer()
	{
		ShouldTransmit = false;
	}

	/// <summary>
	/// Play a timeline.
	/// </summary>
	/// <param name="branch">The timeline branch.</param>
	/// <param name="root">If this is the root branch</param>
	public void PlayTimeline( TimelineBranch branch, bool root = false )
	{
		if ( Entity.ControlMethod != AgentControlMethod.Playback )
		{
			throw new InvalidOperationException( "Pawn must be in Animated mode to play timeline." );
		}

		Entity.AnimPlayer?.Stop();

		if (branch.Weapon.HasValue && Entity.ActiveWeapon == null)
		{
			//Entity.SetActiveWeapon( branch.Weapon.Value.Spawn() );
			var spawner = branch.Weapon.Value;
			Carriable weapon = spawner.Spawn();

			Entity.Inventory.AddItem( weapon );
			Entity.Inventory.ActiveChild = weapon;
		}

		Entity.CreateAnimPlayer().Play( branch.Animation );
		Branch = branch;
		timePlaying = 0;

		if ( root ) RootBranch = branch;
	}

	public void Tick()
	{
		if ( Branch == null ) return;
		if ( Entity.ControlMethod != AgentControlMethod.Playback ) return;

		// Without an end event, we'll just stop here.
		if ( Branch.EndEventTime <= timePlaying && Branch.EndEvent != null )
		{
			var valid = Branch.EndEvent.IsValid( Entity );
			TryPlayBranch( valid ? Branch.BranchA : Branch.BranchB, Branch );
		}
	}

	/// <summary>
	/// Stops the timeline player.
	/// </summary>
	public void Stop()
	{
		Entity.AnimPlayer?.Stop();
		if ( !IsPlaying ) return;
		Branch = null;
	}

	private void TryPlayBranch( TimelineBranch? branch, TimelineBranch? prev = null )
	{
		if ( branch != null )
		{
			PlayTimeline( branch );
		}
		else if ( prev != null )
		{
			Entity.OnUnlink( prev, this );
		}
	}

}
