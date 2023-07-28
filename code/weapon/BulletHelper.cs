#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

public static class BulletHelper
{

	public static BulletInfo FromWeapon(Carriable weapon, float damage, float force = 0, float spread = 0 )
	{
		var ray = weapon.Owner.AimRay;
		ray.Forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * .25f;
		ray.Forward = ray.Forward.Normal;

		return new BulletInfo()
		{
			Ray = ray,
			BaseDamage = damage,
			Force = force
		};
	}
}
