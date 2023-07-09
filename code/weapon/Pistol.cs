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



	public override void DoShootEffects( IEnumerable<PersistentTrace> traces )
	{
		Pawn.PlaySound( "rust_pistol.shoot" );
	}

	public void PrimaryAttack()
	{
		using ( LagCompensation() )
		{
			Shoot( CreateBulletTrace( 0, 2f ) );
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if (Input.Pressed("attack1"))
		{
			PrimaryAttack();
		}
	}
}
