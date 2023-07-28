#nullable enable

using ClockBlockers.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

public static class WeaponTypes
{
	public delegate Carriable WeaponFactory();

	public static readonly string PISTOL = "pistol";
	public static readonly string SHOTGUN = "shotgun";

	public static readonly Dictionary<string, WeaponFactory> REGISTRY = new()
	{
		{ "pistol", () => new Pistol() },
		{ "shotgun", () => new Shotgun() }
	};

	public static WeaponFactory? Get( string id )
	{
		if ( REGISTRY.TryGetValue( id, out WeaponFactory? factory ) )
		{
			return factory;
		}
		else
		{
			return null;
		}
	}

	public struct Spawner
	{
		public WeaponFactory Factory { get; set; }
		public string PersistentID { get; set; }

		public Carriable Spawn()
		{
			var ent = Factory.Invoke();
			ent.SetPersistentID( PersistentID );
			return ent;
		}
	}
}
