using ClockBlockers.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

public delegate Weapon WeaponFactory();

public static class WeaponTypes
{
	public static readonly WeaponFactory PISTOL = () => new Pistol();
	public static readonly WeaponFactory SHOTGUN = () => new Shotgun();
}

public struct WeaponSpawner
{
	public string PersistentID { get; set; }
	public WeaponFactory Factory { get; set; }

	public Weapon Spawn()
	{
		var weapon = Factory.Invoke();
		weapon.SetPersistentID( PersistentID );
		return weapon;
	}
}
