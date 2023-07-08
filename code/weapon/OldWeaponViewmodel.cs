using Sandbox;

namespace ClockBlockers;

public partial class OldWeaponViewmodel : BaseViewModel
{
	protected OldWeapon Weapon { get; init; }

	public OldWeaponViewmodel( OldWeapon weapon )
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
