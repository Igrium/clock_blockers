#nullable enable

using ClockBlockers.Anim;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public static class TestCommands
{
	public static Animation? CachedAnimation { get; set; }

	static Pawn? Caller
	{
		get
		{
			var client = ConsoleSystem.Caller;

			if ( client == null )
			{
				var clients = Game.Clients.Where( c => c.Pawn is Pawn );
				if ( clients.Any() )
				{
					client = clients.First();
				}
			}

			if ( client == null ) return null;

			if ( client.Pawn is Pawn p )
			{
				return p;
			}
			else
			{
				return null;
			}
		}
	}

	[ConCmd.Server( "testcapture_start" )]
	public static void StartCapture()
	{
		var pawn = Caller;
		if ( pawn == null )
		{
			return;
		}

		Log.Info( "Starting capture" );
		pawn.StartCapture();
	}

	[ConCmd.Server( "testcapture_stop" )]
	public static void StopCapture()
	{
		var pawn = Caller;
		if ( pawn == null || !pawn.IsRecording )
		{
			Log.Error( "Pawn is not recording." );
			return;
		}

		Log.Info( "Stopping capture" );
		CachedAnimation = pawn.StopCapture();
	}

	[ConCmd.Server( "testcapture_play" )]
	public static void Play()
	{

		if ( CachedAnimation == null )
		{
			Log.Warning( "There is no cached animation." );
			return;
		}

		Log.Info( "Starting capture playback" );
		AnimPlayer.Create( CachedAnimation );

	}

}
