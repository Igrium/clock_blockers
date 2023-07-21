#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

/// <summary>
/// A barebones movement controller designed to be used with remnants.
/// </summary>
public class PlaybackMovementController : MovementComponent
{
	public override void Simulate( IClient? cl )
	{
		base.Simulate( cl );

		Events.Clear();
		Tags.Clear();
	}

	public void SetSitting()
	{
		SetTag( "sitting" );
	}

	public void SetNoclip()
	{
		SetTag( "noclip" );
	}

	public void SetClimbing()
	{
		SetTag( "climbing" );
	}

	public void SetDucked()
	{
		SetTag( "ducked" );
	}
}
