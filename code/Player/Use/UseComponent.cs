#nullable enable

using ClockBlockers.Anim;
using ClockBlockers.Timeline;
using Sandbox;

namespace ClockBlockers;

public partial class UseComponent : SimulatedComponent
{
	/// <summary>
	/// Entity the player is currently using via their interaction key.
	/// </summary>
	public Entity? Using { get; protected set; }

	/// <summary>
	/// This should be called somewhere in your player's tick to allow them to use entities
	/// </summary>
	public override void Simulate( IClient? cl )
	{
		base.Simulate( cl );

		if ( !Game.IsServer )
			return;

		// Auto stop using if can no longer use.
		if ( !Using.IsValid() || (Using is IUse use && !use.OnUse( Entity )) )
			StopUsing();

		// The rest of this method is only called on the server for players.
		if ( !Game.IsServer || !Entity.HasClient || Entity.ControlMethod != AgentControlMethod.PLAYER )
			return;


		using ( Prediction.Off() )
		{
			var target = Using;
			if ( Input.Pressed( "Use" ) )
			{
				target = FindUsable();

				if ( target == null )
				{
					UseFail();
					return;
				}
			}

			if ( !Input.Down( "use" ) )
			{
				StopUsing();
				return;
			}

			if ( !target.IsValid() )
				return;

			StartUsing( target );
		}
	}

	/// <summary>
	/// Start using an entity.
	/// </summary>
	/// <param name="useTarget">The entity to use.</param>
	/// <returns>If the target entity has a continuous use (key must be held)</returns>
	public bool StartUsing( Entity useTarget )
	{
		// Make sure we're not using something else.
		StopUsing();

		if ( useTarget is not IUse use ) return false;
		bool continuous = use.OnUse( Entity );
		Using = useTarget;

		if ( Entity.IsRecording )
		{
			Entity.AnimCapture?.AddAction( new UseAction()
			{
				TargetID = useTarget.GetPersistentIDOrThrow( generate: true )
			} );
		}

		// Stop using immedietly if this is not continuous.
		if ( !continuous )
			StopUsing( stopAction: false );

		return continuous;
	}

	/// <summary>
	/// Player tried to use something but there was nothing there.
	/// Tradition is to give a disappointed boop.
	/// </summary>
	protected virtual void UseFail()
	{
		Entity.PlaySound( "player_use_fail" );
	}

	/// <summary>
	/// If we're using an entity, stop using it
	/// </summary>
	public virtual void StopUsing(bool stopAction = true)
	{
		if ( Using == null ) return;
		Using = null;
		if (stopAction && Entity.IsRecording)
		{
			Entity.AnimCapture?.AddAction( new StopAction( UseAction.ID ) );
		}
	}

	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	protected bool IsValidUseEntity( Entity e )
	{
		if ( e == null ) return false;
		if ( e is not IUse use ) return false;
		if ( !use.IsUsable( Entity ) ) return false;

		return true;
	}

	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	protected virtual Entity FindUsable()
	{
		// First try a direct 0 width line
		var tr = Trace.Ray( Entity.EyePosition, Entity.EyePosition + Entity.EyeRotation.Forward * 85 )
			.Ignore( Entity )
			.Run();

		// See if any of the parent entities are usable if we ain't.
		var ent = tr.Entity;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			tr = Trace.Ray( Entity.EyePosition, Entity.EyePosition + Entity.EyeRotation.Forward * 85 )
			.Radius( 2 )
			.Ignore( Entity )
			.Run();

			// See if any of the parent entities are usable if we ain't.
			ent = tr.Entity;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		// Still no good? Bail.
		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}
}
