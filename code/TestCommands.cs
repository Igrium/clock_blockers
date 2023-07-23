#nullable enable

using ClockBlockers.Anim;
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

	static Player? Caller
	{
		get
		{
			var pawn = ConsoleSystem.Caller?.Pawn;
			if ( pawn is Player player ) return player;
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

	[ConCmd.Server( "ent_create_ai_agent" )]
	public static void CreateAIAgent()
	{
		Vector3 vec;
		var caller = Caller;

		if ( caller != null )
		{
			var viewTarget = caller.ViewTarget;
			if ( viewTarget.Hit )
			{
				vec = viewTarget.HitPosition;
			}
			else
			{
				Log.Error( "No spawn target" );
				return;
			}
		}
		else
		{
			Log.Error( "No caller pawn" );
			return;
		}

		AgentPawn entity = new AgentPawn();
		entity.Position = vec;
		entity.PostSpawn();

		entity.InitTimeTravel( PawnControlMethod.AI );
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
		var player = new Player();
		player.Inventory?.AddItem( new Pistol() );
		var client = ConsoleSystem.Caller;
		if ( client != null ) client.Pawn = player;


	}

	public static Player PlayAnimation(Animation animation)
	{
		if (animation.IsEmpty)
		{
			throw new ArgumentException( "Animation must not be empty.", "animation" );
		}
		var player = new Player();
		player.SetControlMethod( AgentControlMethod.PLAYBACK );
		player.Inventory.AddItem( new Pistol() );
		player.CreateAnimPlayer().Play( animation );

		return player;
	}
}
