#nullable enable

using ClockBlockers.Timeline;
using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Level;

/// <summary>
/// Causes an unlink if a remnant crosses this entity while its state is
/// different than its canon.
/// </summary>
[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Solid, VisGroup( VisGroup.Trigger ), HideProperty( "enable_shadows" )]
[Title( "Trigger Unlink" ), Icon( "select_all" )]
[Library("trigger_unlink"), HammerEntity]
public partial class TriggerUnlink : BaseTrigger, IHasTimelineState
{

	/// <summary>
	/// The starting state of this trigger. An unlink will be triggered if a
	/// remnant crosses this trigger and the state is different than its canon.
	/// </summary>
	[Property (Title = "State")]
	public int State { get; set; }

	public override void Spawn()
	{
		base.Spawn();
	}

	public int GetState( Player pawn )
	{
		return State;
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );
		if ( toucher is Player pawn ) RecordPawnEvent( pawn );
	}

	public virtual void RecordPawnEvent( Player pawn )
	{
		if ( !Game.IsServer ) return;
		var capture = pawn.TimelineCapture;

		if ( capture == null ) return;

		capture.Event( new MapTimelineEvent
		{
			TriggerID = this.GetPersistentIDOrThrow( true ),
			DesiredState = this.State,
			Name = this.Name
		} );
	}

	/// <summary>
	/// Set the state of this trigger.
	/// </summary>
	/// <param name="state">The new state.</param>
	[Input]
	public void SetState(int state)
	{
		State = state;
	}
}

public struct MapTimelineEvent : ITimelineEvent
{
	public string TriggerID { get; set; }
	public int DesiredState { get; set; }

	public string Name { get; set; }

	public bool IsValid( Player pawn )
	{
		var ent = PersistentEntities.GetEntity<Entity>( TriggerID );
		if ( ent is IHasTimelineState trigger )
		{
			return trigger.GetState( pawn ) == DesiredState;
		}
		else
		{
			return false;
		}
	}
}
