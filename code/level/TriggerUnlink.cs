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
[Library( "trigger_unlink" ), HammerEntity]
public partial class TriggerUnlink : BaseTrigger
{
	/// <summary>
	/// The entity to retrieve the timeline state from.
	/// Must be a valid state provider.
	/// </summary>
	[Property( Title = "Timeline State Provider" )]
	public EntityTarget StateProviderName { get; set; }

	public LogicTimelineState? StateProvider { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		StateProvider = StateProviderName.GetTarget<LogicTimelineState>();
		Log.Info( $"State provider: {StateProviderName.GetTarget()}" );
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );
		if ( toucher is AgentPawn pawn ) RecordPawnEvent( pawn );
	}

	public virtual void RecordPawnEvent( AgentPawn pawn )
	{
		if ( !Game.IsServer ) return;
		if (StateProvider == null)
		{
			Log.Warning( "No state provider was set for " + this );
			return;
		}

		var capture = pawn.TimelineCapture;

		if ( capture == null ) return;

		capture.Event( new MapTimelineEvent
		{
			TriggerID = this.GetPersistentIDOrThrow( true ),
			DesiredState = StateProvider.GetState( pawn ),
			Name = this.Name
		} );
	}
}
