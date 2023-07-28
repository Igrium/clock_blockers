using Sandbox;

namespace ClockBlockers;

public partial class Corpse : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "ragdoll" );
	}

	public DamageInfo KillDamage { get; set; }
	[Net] public Entity? Attacker { get; set; }
	[Net] public Entity? Weapon { get; set; }
	[Net] public IClient? OwnerClient { get; set; }
}
