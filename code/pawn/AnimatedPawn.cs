#nullable enable

using ClockBlockers.Anim;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public partial class Pawn
{
	public AnimPlayer? AnimPlayer { get; protected set; }

	public AnimCapture? AnimCapture { get; protected set; }

	public bool IsRecording => !(AnimCapture == null || !AnimCapture.IsRecording);

	public void PlayAnimation( Animation animation )
	{
		AnimPlayer?.Stop();

		AnimPlayer player = new AnimPlayer();
		player.Animation = animation;
		Components.Add( player );
		AnimPlayer = player;

		player.Start();
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
