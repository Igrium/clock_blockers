using ClockBlockers.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public delegate LegacyWeapon WeaponFactory();

public static class LegacyWeaponTypes
{
	public static readonly WeaponFactory PISTOL = () => new LegacyPistol();
	public static readonly WeaponFactory SHOTGUN = () => new LegacyShotgun();
}

public struct LegacyWeaponSpawner
{
	public string PersistentID { get; set; }
	public WeaponFactory Factory { get; set; }

	public LegacyWeapon Spawn()
	{
		var weapon = Factory.Invoke();
		weapon.SetPersistentID( PersistentID );
		return weapon;
	}
}
