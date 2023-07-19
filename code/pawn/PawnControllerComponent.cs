#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public class PawnControllerComponent : EntityComponent<AgentPawn>
{
	public static readonly float MOVEMENT_SPEED = 320;

	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 300;
	public float Gravity => 800f;

	bool Grounded => Entity.GroundEntity.IsValid();

	/// <summary>
	/// Whether the pawn jumped this tick.
	/// </summary>
	public bool DidJump { get; protected set; }

	public bool Running { get; set; }

	public void Simulate()
	{
		DidJump = false;

		var movement = Entity.MovementDirection.Normal;
		var moveVector = movement * MOVEMENT_SPEED;
		var groundEntity = CheckForGround();

		if ( groundEntity != null && groundEntity.IsValid )
		{
			if ( !Grounded )
			{
				Entity.Velocity = Entity.Velocity.WithZ( 0 );
			}

			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 200f * ( Running ? 2.5f : 1f ), 7.5f );
			Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );
		} 
		else
		{
			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f );
			Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
		}
		

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity ).WithoutTags( "player" );

		if ( mh.TryMoveWithStep(Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
		Entity.IsGrounded = Grounded;
	}

	/// <summary>
	/// Attempt to jump.
	/// </summary>
	/// <returns>Whether the jump was successful.</returns>
	public bool DoJump()
	{
		if ( Grounded && !DidJump )
		{
			Entity.Velocity += Vector3.Up * JumpSpeed;
			DidJump = true;
			Entity.DoJumpAnimation();
			return true;
		}
		return false;
	}


	Entity? CheckForGround()
	{
		if ( Entity.Velocity.z > 100f ) return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit ) return null;
		if ( trace.Normal.Angle( Vector3.Up ) > GroundAngle ) return null;

		return trace.Entity;
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishSpeed > speedLimit )
			wishSpeed = speedLimit;

		var currentSpeed = input.Dot( wishdir );
		var addSpeed = wishSpeed - currentSpeed;

		if ( addSpeed <= 0 ) return input;

		var accelSpeed = acceleration * Time.Delta * wishSpeed;

		if ( accelSpeed > addSpeed )
			accelSpeed = addSpeed;

		input += wishdir * accelSpeed;
		return input;
	}

	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		float StopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return input;

		newspeed /= speed;
		input *= newspeed;

		return input;
	}

	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return position;
		if ( trace.Fraction >= 1 ) return position;
		if ( trace.StartedSolid ) return position;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return position;

		return trace.EndPosition;
	}
}
