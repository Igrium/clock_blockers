#nullable enable

using ClockBlockers.Timeline;
using Sandbox;
using Sandbox.ModelEditor;

namespace ClockBlockers.Weapon;

/// <summary>
/// The info needed to fire a bullet.
/// </summary>
public struct BulletInfo
{
	public Ray Ray { get; set; }
	public float BaseDamage { get; set; }
	public float FalloffMultiplier { get; set; }
	public DamageFalloffType FalloffType { get; set; }
	public float Force { get; set; }
	public float Radius { get; set; }	
}
