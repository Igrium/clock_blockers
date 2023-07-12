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

public partial class Pawn
{
	public AnimPlayer AnimPlayer { get; protected set; } = new AnimPlayer();

	public TimelinePlayer TimelinePlayer { get; protected set; } = new TimelinePlayer();

	public AnimCapture? AnimCapture { get; protected set; }

	public bool IsRecording => !(AnimCapture == null || !AnimCapture.IsRecording);

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
		if (AnimCapture == null)
		{
			throw new InvalidOperationException( "Pawn is not capturing." );
		}
		var anim = AnimCapture.Stop();
		AnimCapture = null;
		return anim;
	}
}
