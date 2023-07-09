using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ClockBlockers;

public partial class Firearm
{
	/// <summary>
	/// Create a <c>TraceInfo</c> appropriate for a bullet.
	/// </summary>
	/// <param name="start">Start position</param>
	/// <param name="end">End position</param>
	/// <param name="radius">Bullet size</param>
	/// <returns>The trace</returns>
	public static TraceInfo CreateBulletTrace(Vector3 start, Vector3 end, float radius = 2f)
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = new TraceInfo()
		{
			Start = start,
			End = end,
			Radius = radius,
			Tags = new string[] { "solid", "player", "npc", "glass" }
		};

		if ( !underWater ) trace.WithAnyTags( "water" );

		return trace;
	}

	public static TraceInfo CreateBulletTrace( Vector3 pos, Vector3 dir, float spread, float radius = 2f )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		var end = forward * 5000;

		return CreateBulletTrace( pos, end, radius );
	}

	public static TraceInfo CreateBulletTrace( Entity owner, float spread, float radius = 2f )
	{
		var ray = owner.AimRay;
		return CreateBulletTrace( ray.Position, ray.Forward, spread, radius );
	}
}
