﻿#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.AI;

/// <summary>
/// When agents are unlinked, they get controlled by AI.
/// This class is the AI they get controlled by.
/// </summary>
public partial class AIController : EntityComponent<Pawn>
{
	public static readonly float TARGET_INTERVAL = 1;

	public AIController()
	{
		ShouldTransmit = false;
	}

	// Temporary AI for testing

	private Entity? _target;

	private TimeSince _lastTargetUpdate;

	public Entity? Target
	{
		get => _target; set
		{
			_target = value;
			_lastTargetUpdate = 0;
		}
	}

	public void Tick()
	{
		// Reset any movement from the previous tick.
		Entity.MovementDirection = new Vector3();

		if ( _lastTargetUpdate > TARGET_INTERVAL ) UpdateTarget();
		if ( Target == null ) return;

		TickMovement();
	}

	public void UpdateTarget()
	{
		Log.Trace( "Updating AI target" );
		Entity? closest = null;

		foreach ( var ent in Sandbox.Entity.All.Where( CanTarget ) )
		{
			if ( closest == null )
			{
				closest = ent;
				continue;
			}

			if ( ent.Position.DistanceSquared( Entity.Position ) < closest.Position.DistanceSquared( Entity.Position ) )
			{
				closest = ent;
			}
		}

		Target = closest;
	}

	protected virtual bool CanTarget( Entity target )
	{
		if ( target is Pawn pawn )
		{
			return pawn.ControlMethod == Pawn.PawnControlMethod.Player;
		}
		else return false;
	}
}