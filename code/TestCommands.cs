﻿#nullable enable

using ClockBlockers.Anim;
using ClockBlockers.Spectator;
using ClockBlockers.Weapon;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClockBlockers;

public static class TestCommands
{
	public static Animation? CachedAnimation { get; set; }

	static PlayerAgent? Caller
	{
		get
		{
			var pawn = ConsoleSystem.Caller?.Pawn;
			if ( pawn is PlayerAgent player ) return player;
			else return null;
		}
	}
	

	[ConCmd.Server( "testcapture_start" )]
	public static void StartCapture()
	{
		var pawn = Caller;
		if ( pawn == null )
		{
			Log.Error( "No pawn" );
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
			Log.Error( $"Pawn {pawn} is not recording." );
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
		PlayAnimation( CachedAnimation );

	}

	[ConCmd.Server( "game_reset" )]
	public static void GameReset()
	{
		var ignoreEntities = Entity.All.Where( e => e.Owner != null ).ToArray();
		Game.ResetMap( ignoreEntities );
	}

	[ConCmd.Server( "round_start" )]
	public static void StartRound()
	{
		var game = Entity.All.OfType<ClockBlockersGame>().FirstOrDefault();
		if ( game == null )
		{
			Log.Error( "No game." );
			return;
		}
		if ( game.Round != null )
		{
			Log.Error( "Game is already in a round." );
			return;
		}

		game.DoRound();
	}

	[ConCmd.Server("spawn_player")]
	public static void TestPlayer()
	{
		var player = new PlayerAgent();
		player.Inventory?.AddItem( new Shotgun() );
		var client = ConsoleSystem.Caller;

		if ( client != null )
		{
			if (client.Pawn is SpectatorPawn)
			{
				client.Pawn.Delete();
			}
			client.Pawn = player;
		}
	}

	public static PlayerAgent PlayAnimation(Animation animation)
	{
		if (animation.IsEmpty)
		{
			throw new ArgumentException( "Animation must not be empty.", "animation" );
		}
		var player = new PlayerAgent();
		player.SetControlMethod( AgentControlMethod.Playback );
		player.Inventory.AddItem( new Pistol() );
		player.CreateAnimPlayer().Play( animation );

		return player;
	}
}
