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

	public static AnimCapture? CurrentCapture { get; set; }

	public static AnimPlayer? CurrentPlayer { get; set; }

	[ConCmd.Server( "testcapture_start" )]
	public static void StartCapture()
	{
		var client = ConsoleSystem.Caller;

		if (client == null)
		{
			var clients = Game.Clients.Where( c => c.Pawn is Pawn );
			if ( clients.Any() )
			{
				client = clients.First();
			}
		}

		if ( client == null ) return;

		Pawn pawn;
		if ( client.Pawn is Pawn p )
		{
			pawn = p;
		}
		else
		{
			return;
		}

		if ( CurrentCapture != null )
		{
			StopCapture();
		}

		CurrentCapture = new AnimCapture( pawn );
		Log.Info( "Starting capture" );
		CurrentCapture.Start();
	}

	[ConCmd.Server( "testcapture_stop")]
	public static void StopCapture()
	{
		if ( CurrentCapture == null ) return;
		CachedAnimation = CurrentCapture.Stop();
		CurrentCapture = null;

		Log.Info( "Stopping capture" );
	}

	[ConCmd.Server( "testcapture_play" )]
	public static void Play()
	{

		CurrentPlayer?.Stop();
		if (CachedAnimation == null)
		{
			Log.Warning( "There is no cached animation." );
			return;
		}

		Log.Info( "Starting capture playback" );
		CurrentPlayer = AnimPlayer.Create( CachedAnimation );
		CurrentPlayer.Start();

	}

	[GameEvent.Tick.Server]
	static void Tick()
	{
		CurrentCapture?.Tick();
		CurrentPlayer?.Tick();
	}
}
