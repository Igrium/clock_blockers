#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public class WeaponViewModel : BaseViewModel
{
	protected LegacyWeapon Weapon { get; init; }
	
	public WeaponViewModel( LegacyWeapon weapon )
	{
		Weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
	}

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );
	}
}
