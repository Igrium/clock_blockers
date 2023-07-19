#nullable enable

using ClockBlockers.Anim;
using ClockBlockers.Spectator;
using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

/*
 * The base code for agents.
 */

/// <summary>
/// How is this pawn being controlled?
/// </summary>
public enum PawnControlMethod
{
	/// <summary>
	/// Controlled by a player. If no player possesses this entity, does nothing.
	/// </summary>
	Player,

	/// <summary>
	/// Animated remnant from a previous round.
	/// </summary>
	Animated,

	/// <summary>
	/// Controlled by AI.
	/// </summary>
	AI
}

/// <summary>
/// An agent on the field, be it a remnant, an AI pawn, or a player.
/// </summary>
public partial class AgentPawn : AnimatedEntity
{

	[Net]
	public PawnControlMethod ControlMethod { get; set; } = PawnControlMethod.Player;

	[Net, Predicted]
	public Weapon? ActiveWeapon { get; private set; }

	// TODO: Store / calculate input direction globally so it can manipulated easier.
	[ClientInput]
	public Vector3 MovementDirection { get; set; }

	[ClientInput]
	public Angles ViewAngles { get; set; }

	// Store IsGrounded manually so animations can set it.
	[Predicted]
	public bool IsGrounded { get; set; }

	public float UseDistance { get; set; } = 64;

	/// <summary>
	/// The client possessing this agent.
	/// Unlike <c>Client</c>, this checks the client still uses this pawn.
	/// </summary>
	public IClient? Possessor
	{
		get
		{
			if ( Client != null && Client.Pawn == this ) return Client;
			return null;
		}
	}

	/// <summary>
	/// The current crosshair target of this player.
	/// </summary>
	public TraceResult ViewTarget
	{
		get
		{
			Trace traceInfo = Firearm.CreateBulletTrace( this ).CreateTrace();
			return traceInfo.Run();
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "player" );

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		PhysicsBody.UseController = true;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Components.Create<PawnControllerComponent>();
		Components.Create<PawnAnimatorComponent>();

		AnimPlayer = Components.Create<AnimPlayer>();
		TimelinePlayer = Components.Create<TimelinePlayer>();


		EnableTraceAndQueries = true;
		EnableTouch = true;

		Health = 100f;
	}

	public void PostSpawn()
	{
	}

	/// <summary>
	/// Make this pawn play the "jump" animation.
	/// Does NOT make the pawn actionally jump (see controller)
	/// </summary>
	public void DoJumpAnimation()
	{
		DidJump = true;
		AnimCapture?.AddAction( IAction.Jump() );
	}

	public bool DidJump { get; protected set; }

	private ClothingContainer? _clothing;

	public ClothingContainer? Clothing
	{
		get => _clothing; set
		{
			//if ( _clothing != null )
			//{
			//	_clothing.ClearEntities();
			//}
			if ( value == null )
			{
				_clothing = null;
				return;
			}
			_clothing = new ClothingContainer();
			_clothing.Deserialize( value.Serialize() );
			if ( _clothing != null )
			{
				_clothing.DressEntity( this );
			}
		}
	}

	/// <summary>
	/// Position a player should be looking from in global space.
	/// </summary>
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local space.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// The rotation of the camera for this entity, in world space.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// The rotation of the camera for this entity, in local space.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new BBox
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public PawnControllerComponent? Controller { get; }
	[BindComponent] public PawnAnimatorComponent? Animator { get; }

	/// <summary>
	/// Whether this pawn is a "free agent" (not following a set timeline)
	/// </summary>
	public bool IsFreeAgent => ControlMethod != PawnControlMethod.Animated;

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public void SetActiveWeapon( Weapon? weapon )
	{
		if ( weapon?.Owner != null && weapon?.Owner != this )
		{
			throw new InvalidOperationException( "The selected weapon already has an owner." );
		}

		ActiveWeapon?.OnHolster();
		ActiveWeapon = weapon;
		ActiveWeapon?.OnEquip( this );

		if ( ActiveWeapon != null )
		{
			SetAnimParameter( "holdtype", (int)ActiveWeapon.HoldType );
		}
		else
		{
			SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.None );
		}
	}

	/// <summary>
	/// The velocity to apply to dropped weapons.
	/// </summary>
	protected static readonly Vector3 DROP_VELOCITY = new Vector3( 128, 0, 192 );

	/// <summary>
	/// Drop the currently-held weapon if possible.
	/// </summary>
	/// <param name="applyVelocity">Velocity to apply to the weapon after drop.</param>
	/// <param name="recordAction">Record this as an action.
	/// Should only be false if this is part of a weapon pickup, which has its own action.</param>
	public void DropWeapon( Vector3 applyVelocity = new Vector3(), bool recordAction = true )
	{
		if ( ActiveWeapon == null || !ActiveWeapon.CanDrop ) return;

		ActiveWeapon.OnDrop();
		SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.None );

		ActiveWeapon.Velocity += applyVelocity.RotateAround( new Vector3( 0, 0, 0 ), Transform.Rotation );
		ActiveWeapon = null;


		if ( recordAction )
			AnimCapture?.AddAction( new DropWeaponAction { Velocity = applyVelocity } );
	}


	public void PickUpWeapon( Weapon weapon )
	{
		if (weapon.IsHeld)
		{
			throw new InvalidOperationException( "You may only pick up a weapon from the ground." );
		}

		if ( ActiveWeapon != null )
		{
			if ( !ActiveWeapon.CanDrop )
			{
				Log.Warning( "The current weapon cannot be dropped." );
				return;
			}
			DropWeapon( DROP_VELOCITY, recordAction: false );
		}

		TimelineCapture?.Event( new PickupWeaponEvent()
		{
			WeaponID = weapon.GetPersistentIDOrThrow( true ),
			Position = weapon.Position
		} );

		SetActiveWeapon( weapon );
		AnimCapture?.AddAction( new PickUpWeaponAction( weapon ) );
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		this.Clothing = c;
	}

	public override void Simulate( IClient cl )
	{
		if ( ControlMethod != PawnControlMethod.Player ) return;
		if ( LifeState != LifeState.Alive ) return;

		if ( Controller != null )
		{
			if ( Input.Pressed( "jump" ) ) Controller.DoJump();
			Controller.Running = Input.Down( "run" );
			Controller.Simulate();

		}
		ActiveWeapon?.Simulate( cl );

		if ( Input.Pressed( "use" ) ) TryUse();

		if ( Input.Pressed( "Drop" ) ) DropWeapon( DROP_VELOCITY );

		TickAll( cl );

	}

	[GameEvent.Tick.Server]
	public void TickServer()
	{
		// Already handled in Simulate
		if ( ControlMethod == PawnControlMethod.Player ) return;
		TickAll();
	}

	/// <summary>
	/// Called every tick on the server no matter what. If possessed on a player, also called on the client.
	/// Calls during <c>Simulate</c> if possessed by a client and <c>GameEvent.Tick.Player</c> if not.
	/// </summary>
	/// <param name="cl">The client, if possessed by a player</param>
	public void TickAll( IClient? cl = null )
	{
		if ( LifeState != LifeState.Alive ) return;

		SimulateRotation();
		AnimPlayer?.Tick();
		EyeLocalPosition = Vector3.Up * (64f * Scale);
		AnimCapture?.Tick();
		TimelinePlayer?.Tick();
		Animator?.Animate();

		ActiveWeapon?.Tick();

		// Reset for next frame
		DidJump = false;

	}

	public override void BuildInput()
	{
		if ( LifeState != LifeState.Alive ) return;

		var moveDirection = Input.AnalogMove;

		if ( Input.StopProcessing ) return;

		var look = Input.AnalogLook;
		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			// TODO: figure out what this is for
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0;
		ViewAngles = viewAngles.Normal;

		// Calculate movement direction
		moveDirection = moveDirection.Normal;
		var angles = viewAngles.WithPitch( 0 );
		MovementDirection = Rotation.From( angles ) * moveDirection;
	}

	bool IsThirdPerson { get; set; } = false;

	public override void FrameSimulate( IClient cl )
	{
		if ( ControlMethod == PawnControlMethod.Player ) SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( Input.Pressed( "view" ) )
		{
			IsThirdPerson = !IsThirdPerson;
		}

		if ( IsThirdPerson )
		{
			// Calculate third person camera pos
			Vector3 targetPos;
			var pos = Position + Vector3.Up * 64;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 80f * Scale;
			targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();

			Camera.FirstPersonViewer = null;
			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}
	}

	protected virtual void OnUseFail()
	{
		PlaySound( "player_use_fail" );
	}

	/// <summary>
	/// Find a usable entity for this player to use.
	/// </summary>
	protected virtual Entity? FindUsable()
	{
		// First try a direct 0 width line
		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 85 )
			.Ignore( this )
			.WithoutTags("player")
			.Run();

		// Get the first usable parent
		var ent = tr.Entity;
		while ( !IsValidUseEntity( ent ) )
		{
			if ( ent == null ) return null;
			ent = ent.Parent;
		}

		// Try a wider search if nothing found.
		if ( !IsValidUseEntity( ent ) )
		{
			tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 64 )
				.Radius( 4f )
				.Ignore( this )
				.WithoutTags("player")
				.Run();

			ent = tr.Entity;
			while ( !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}

	protected bool IsValidUseEntity( Entity? e )
	{
		if ( e is not IUse use ) return false;
		if ( !e.IsValid ) return false;

		if ( !use.IsUsable( this ) ) return false;
		return true;
	}

	public void TryUse()
	{
		if ( !Game.IsServer ) return;
		using ( Prediction.Off() )
		{
			using ( LagCompensation() )
			{
				var target = FindUsable();
				if ( target == null )
				{
					OnUseFail();
					return;
				}
				Use( target );
			}
		}
	}

	public virtual void Use( Entity target )
	{
		if ( !Game.IsServer ) return;

		if ( target is not IUse use )
			throw new ArgumentException( "The target must implement IUse", "target" );

		// TODO: Fix this
		//if ( TimelineCapture != null )
		//{
		//	var e = new UseEvent( target );

		//	if ( target is IHasTimelineState stateHolder && stateHolder.RequireUseStateMatch( this ) )
		//	{
		//		e.DesiredState = stateHolder.GetState( this );
		//	}

		//}

		if ( AnimCapture != null && target is not IUseNotCanon )
		{
			AnimCapture.AddAction( new UseAction( target ) );
		}

		use.OnUse( this );
	}

	public override void OnKilled()
	{
		if ( LifeState != LifeState.Alive ) return;
		LifeState = LifeState.Dead;
		DropWeapon( recordAction: false );

		var client = Possessor;
		if ( client != null )
		{
			var spectator = SpectatorPawn.Create();
			client.Pawn = spectator;
		}
		TimelinePlayer?.Stop();


		// Ragdoll
		var vel = this.Velocity;
		PhysicsEnabled = true;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		Velocity = vel;

		// So that unlinks happen if not killed in the future
		TimelineCapture?.Event( new DeathEvent(), final: true );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WithAnyTags( "solid", "playerclip", "passbullets" )
			.Ignore( this )
			.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}

	public void EndRound()
	{
		SetActiveWeapon( null );
		FinalizeAnimations();
	}
}
