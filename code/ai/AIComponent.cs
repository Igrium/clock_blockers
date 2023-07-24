#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.AI;

public partial class AIComponent : SimulatedComponent, ISingletonComponent
{
	public float PathGenInterval { get; set; } = .5f;
	public float MaxClimbDistance { get; set; } = 16f;
	public float MaxDropDistance { get; set; } = 128;
	public float MaxStepHeight { get; set; } = 16f;

	/// <summary>
	/// When set, the agent will move towards this target in a straight line,
	/// disregarding any obsticals. Used internally to navigate.
	/// </summary>
	public Vector3? MovementTarget { get; protected set; }

	/// <summary>
	/// Whether this agent should crouch if not moving.
	/// </summary>
	public bool IsCrouched { get; set; }

	private IPathTarget? _pathTarget;

	public IPathTarget? PathTarget
	{
		get => _pathTarget; set
		{
			_pathTarget = value;

		}
	}

	/// <summary>
	/// The current path that the agent is traversing.
	/// </summary>
	public NavPath? NavPath { get; protected set; }

	protected TimeSince LastPathGenerated = 100f;
	protected int CurrentPathSegment { get; private set; }

	public AIComponent()
	{
		ShouldTransmit = false;
	}

	public NavPath? CurrentPath { get; protected set; }

	/// <summary>
	/// Whether this entity should use the nav mesh for navigating.
	/// If false, will move directly towards target.
	/// </summary>
	public bool UseNavMesh { get; set; } = true;

	public override void Simulate( IClient? cl )
	{
		base.Simulate( cl );
		if ( Entity.MovementController is not AIMovementController moveController ) return;

		TickMovement( moveController );
	}

	private void CheckMoveType( MovementType moveType, AIMovementController moveController)
	{
		if (moveType == MovementType.Sprint)
		{
			moveController.WantsDuck = false;
			moveController.IsWalking = false;
			moveController.IsSprinting = true;
		}
		else if (moveType == MovementType.Walk)
		{
			moveController.WantsDuck = false;
			moveController.IsWalking = true;
			moveController.IsSprinting = false;
		}
		else if (moveType == MovementType.Duck)
		{
			moveController.WantsDuck = true;
			moveController.IsWalking = false;
			moveController.IsSprinting = false;
		}
		else
		{
			moveController.WantsDuck = false;
			moveController.IsWalking = false;
			moveController.IsSprinting = false;
		}
	}

	public void TickMovement(AIMovementController moveController)
	{
		TraversePath( moveController );
		if ( MovementTarget.HasValue )
		{
			moveController.WishVelocity = (MovementTarget.Value - Entity.Position).Normal;
		}
	}

	protected void UpdatePath( Vector3 target )
	{
		LastPathGenerated = 0;

		NavPath = new NavPathBuilder(Entity.Position)
			.WithMaxClimbDistance( MaxClimbDistance )
			.WithMaxDropDistance( MaxDropDistance )
			.WithStepHeight( MaxStepHeight )
			.WithPartialPaths()
			.Build( target );


		if ( NavPath == null)
		{
			Log.Warning( $"{Entity} was unable to find a partial path to {target}" );
		}

		CurrentPathSegment = 0;
	}

	protected void TraversePath( AIMovementController moveController )
	{
		if ( PathTarget == null )
		{
			NavPath = null;
			return;
		}
		Vector3 pathTargetPos = PathTarget.CurrentPosition();

		if ( Entity.Position.Distance( pathTargetPos ) <= PathTarget.TargetEpsilon )
		{
			PathTarget = null;
			NavPath = null;
			return;
		}

		CheckMoveType( PathTarget.MoveType, moveController );

		if ( !UseNavMesh )
		{
			MovementTarget = pathTargetPos;
			return;
		}

		if (LastPathGenerated > PathGenInterval)
		{
			UpdatePath( pathTargetPos );
		}

		if ( NavPath == null )
		{
			MovementTarget = pathTargetPos;
			return;
		}

		var navSegments = NavPath.Segments;
		// If we're on the final path segment, move directly towards the target.
		if ( CurrentPathSegment >= navSegments.Count - 1 )
		{
			MovementTarget = pathTargetPos;
		}
		else
		{
			MovementTarget = navSegments[CurrentPathSegment].Position;
		}

		if (Entity.Position.Distance(MovementTarget.Value) <= PathTarget.TargetEpsilon)
		{
			CurrentPathSegment++;
		}
	}

}
