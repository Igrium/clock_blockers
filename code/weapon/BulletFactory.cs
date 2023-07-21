using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

/// <summary>
/// All the values needed to recreate a trace consistently.
/// </summary>
public struct BulletFactory
{
	public Vector3 From { get; set; }
	public Vector3 To { get; set; }
	public float Damage { get; set; }
	public float Force { get; set; }
	public float Radius { get; set; }
	public Vector3? TracerPosition { get; set; }
	public string TracerOverride { get; set; }

	public BulletFactory()
	{
	}

	/// <summary>
	/// Create a real trace from this trace factory.
	/// </summary>
	/// <returns>The trace.</returns>
	public Trace GetTrace()
	{
		Trace trace = Trace.Ray( From, To )
			.Radius( Radius )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc", "penetrable", "corpse", "glass", "water", "carriable", "debris" )
			.WithoutTags( "trigger", "skybox", "playerclip" );

		return trace;
	}

}
