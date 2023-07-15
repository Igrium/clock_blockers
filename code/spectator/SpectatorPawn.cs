using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Spectator;

public class SpectatorPawn : ModelEntity
{
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		//
		// Use a watermelon model
		//
		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

		EnableTouch = true;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Tags.Add( "player" );
	}

	// An example BuildInput method within a player's Pawn class.
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;
		ViewAngles = viewAngles.Normal;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Rotation = ViewAngles.ToRotation();

		// build movement from the input values
		var movement = InputDirection.Normal;

		// rotate it to the direction we're facing
		Velocity = Rotation * movement;

		// apply some speed to it
		Velocity *= Input.Down( "run" ) ? 1000 : 200;

		// apply it to our position using MoveHelper, which handles collision
		// detection and sliding across surfaces for us
		MoveHelper helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 16 ).Ignore( this );
		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Position = helper.Position;
		}

		// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
		if ( Game.IsServer && Input.Pressed( "attack1" ) )
		{
			var ragdoll = new ModelEntity();
			ragdoll.SetModel( "models/citizen/citizen.vmdl" );
			ragdoll.Position = Position + Rotation.Forward * 40;
			ragdoll.Rotation = Rotation.LookAt( Vector3.Random.Normal );
			ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			ragdoll.PhysicsGroup.Velocity = Rotation.Forward * 1000;
		}
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		// Update rotation every frame, to keep things smooth
		Rotation = ViewAngles.ToRotation();

		Camera.Position = Position;
		Camera.Rotation = Rotation;

		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		// Set the first person viewer to this, so it won't render our model
		Camera.FirstPersonViewer = this;
	}

	public static SpectatorPawn SpawnEntity()
	{
		var specPawn = new SpectatorPawn();

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			specPawn.Transform = tx;
		}
		return specPawn;
	}
}
