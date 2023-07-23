#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

public static class BulletHelper
{
	public static BulletInfo FromWeapon( Carriable weapon, float damage, float force = 0 )
	{
		return new BulletInfo()
		{
			Ray = weapon.Owner.AimRay,
			BaseDamage = damage,
			Force = force
		};
	}
}
