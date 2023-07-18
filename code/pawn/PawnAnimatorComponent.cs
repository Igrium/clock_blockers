#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public class PawnAnimatorComponent : EntityComponent<AgentPawn>, ISingletonComponent
{
	public void Animate()
	{
		var helper = new CitizenAnimationHelper( Entity );
		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.IsGrounded;

		if ( Entity.DidJump ) helper.TriggerJump();
	}
	
}
