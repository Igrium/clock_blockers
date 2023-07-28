#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.AI;

public partial class FollowAIController : AIComponent
{
	public float TargetUpdateInterval { get; set; } = 4f;
	public Entity? Target { get; set; }
	public TimeSince TimeSinceTargetUpdate { get; private set; } = 100;

	public override void Simulate( IClient? cl )
	{
		if ( Entity.ControlMethod != AgentControlMethod.AI ) return;
		DebugOverlay.ScreenText( $"Target: {Target}" );

		if ( TimeSinceTargetUpdate > TargetUpdateInterval )
		{
			UpdateTarget();
			if ( Target != null )
			{
				PathTarget = new EntityPathTarget( Target );
			}

		}

		base.Simulate( cl );
		if (Entity.MovementController is AIMovementController moveController && Target != null)
		{
			moveController.LookAt( Target.Position );
		}
	}

	public void UpdateTarget()
	{
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
		TimeSinceTargetUpdate = 0;
	}

	protected virtual bool CanTarget( Entity target )
	{
		if ( target is PlayerAgent pawn )
		{
			return pawn.IsFreeAgent && pawn.ControlMethod == AgentControlMethod.Player;
		}
		else return false;
	}
}
