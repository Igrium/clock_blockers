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
	private Entity? _prevUsing;

	/// <summary>
	/// This should be called somewhere in your player's tick to allow them to use entities
	/// </summary>

	public override void Simulate( IClient? cl )
	{
		base.Simulate( cl );

		if ( !Game.IsServer ) return;
		using ( Prediction.Off() )
		{
			TickUse();
		}
	}

	public void TickUse()
	{
		// We only check inputs if we are controlled by a player.
		if ( Entity.HasClient && Entity.ControlMethod == AgentControlMethod.PLAYER )
		{
			if ( Input.Pressed( "use" ) )
			{
				Using = FindUsable();
				if ( Using == null )
				{
					UseFail();
					return;
				}
			}
			if ( !Input.Down( "use" ) )
			{
				StopUsing( true );
				return;
			}
		}

		// We are no-longer able to use this entity.
		if ( !Using.IsValid() || Using is not IUse use )
		{
			// Only create a stop action if we were already using.
			StopUsing( _prevUsing == Using );
			return;
		}

		bool continuous = use.OnUse( Entity );

		if ( Entity.IsRecording && _prevUsing != Using )
		{
			Entity.AnimCapture?.AddAction( new UseAction( Using )
			{
				Continuous = continuous
			} );
		}

		_prevUsing = Using;
		if ( !continuous )
		{
			StopUsing( false );
		}

	}

	public void StartUsing( Entity useTarget )
	{
		Using = useTarget;
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
	public virtual void StopUsing( bool stopAction = true )
	{
		_prevUsing = null;
		if ( Using == null ) return;
		Using = null;

		if ( stopAction && Entity.IsRecording )
		{
			Entity.AnimCapture?.AddAction( new StopAction( UseAction.ID ) );
		}

	}

	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	protected bool IsValidUseEntity( Entity? e )
	{
		if ( !e.IsValid() ) return false;
		if ( e is not IUse use ) return false;
		if ( !use.IsUsable( Entity ) ) return false;

		return true;
	}

	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	protected virtual Entity? FindUsable()
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
