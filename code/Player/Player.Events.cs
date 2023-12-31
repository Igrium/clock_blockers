﻿using Sandbox;

namespace ClockBlockers;

public partial class PlayerAgent
{
	// Empty overrides for events.
	public override void StartTouch( Entity other )
	{
		base.Touch( other );
		Event.Run( "Player.StartTouch", other, this );
	}
	public override void Touch( Entity other )
	{
		base.Touch( other );
		Event.Run( "Player.Touch", other, this );
	}
	public override void EndTouch( Entity other )
	{
		base.Touch( other );
		Event.Run( "Player.EndTouch", other, this );
	}
}
