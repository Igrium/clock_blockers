using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public partial class Pistol : Firearm
{
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Pistol;

	public bool DidShoot { get; private set; }

	public override void DoShootEffects( IEnumerable<TraceInfo> traces )
	{
		Pawn.PlaySound( "rust_pistol.shoot" );

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Pawn.SetAnimParameter( "b_attack", true );
	}

	public void PrimaryAttack()
	{
		DidShoot = true;
		using ( LagCompensation() )
		{
			Shoot( CreateBulletTrace( this.Owner ) );
		}
	}

	public override void Simulate( IClient cl )
	{
		DidShoot = false;
		base.Simulate( cl );

		if (Input.Pressed("attack1"))
		{
			PrimaryAttack();
		}
	}

	public override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
