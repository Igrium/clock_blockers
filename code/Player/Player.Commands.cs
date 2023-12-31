﻿using Sandbox;

namespace ClockBlockers;
public partial class PlayerAgent
{
	[ConCmd.Admin( "noclip" )]
	static void DoPlayerNoclip()
	{
		if ( ConsoleSystem.Caller.Pawn is PlayerAgent basePlayer )
		{
			if ( basePlayer.MovementController is NoclipController )
			{
				basePlayer.Components.Add( new WalkController() );
			}
			else
			{
				basePlayer.Components.Add( new NoclipController() );
			}
		}
	}

	[ConCmd.Admin( "kill" )]
	static void DoPlayerSuicide()
	{
		if ( ConsoleSystem.Caller.Pawn is PlayerAgent basePlayer )
		{
			basePlayer.TakeDamage( new DamageInfo { Damage = basePlayer.Health * 99 } );
		}
	}
	[ConCmd.Admin( "respawn" )]
	static void DoPlayerRespawn()
	{
		if ( ConsoleSystem.Caller.Pawn is PlayerAgent basePlayer )
		{
			basePlayer.Respawn();
		}
	}
}
