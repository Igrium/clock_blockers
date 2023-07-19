using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public partial class Shotgun : Firearm
{
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun";

	public override string WorldModelPath => "weapons/rust_pumpshotgun/rust_pumpshotgun";

	public override CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Shotgun;

	private static readonly int SHOT_COUNT = 9;

	public static readonly float FIRE_INTERVAL = 1.5f;

	protected TimeSince LastFire { get; set; }

	public void PrimaryFire()
	{
		if ( LastFire < FIRE_INTERVAL ) return;

		TraceInfo[] traces = new TraceInfo[SHOT_COUNT];
		for (int i = 0; i < SHOT_COUNT; i++)
		{
			traces[i] = CreateBulletTrace( Owner, spread: .25f );
		}

		using ( LagCompensation() )
		{
			Shoot( traces );
		}
		LastFire = 0;
	}

	public override void DoShootEffects( IEnumerable<TraceInfo> traces )
	{
		Pawn.PlaySound( "rust_pumpshotgun.shoot" );

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Pawn.SetAnimParameter( "b_attack", true );
		ViewModel?.SetAnimParameter( "fire_double", true );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		if (Input.Pressed("attack1"))
		{
			PrimaryFire();
		}

	}
}
