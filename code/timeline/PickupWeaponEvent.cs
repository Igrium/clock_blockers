using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

public struct PickupWeaponEvent : ITimelineEvent
{
	/// <summary>
	/// The max distance the weapon may be from its recorded position horizontally.
	/// </summary>
	public static readonly float MAX_ERROR = 64;

	/// <summary>
	/// The max distance the weapon may be from its recorded position vertically.
	/// </summary>
	public static readonly float MAX_HEIGHT_ERROR = 32;

	/// <summary>
	/// The persistent ID of the weapon to pick up.
	/// </summary>
	public string WeaponID { get; set; }

	/// <summary>
	/// The position the weapon was at when it was picked up.
	/// </summary>
	public Vector3 Position { get; set; }

	public string Name => $"Pickup weapon {WeaponID}";

	public PickupWeaponEvent(Entity weapon)
	{
		WeaponID = weapon.GetPersistentIDOrCreate();
		Position = weapon.Position;
	}

	public bool IsValid( Player pawn )
	{
		if ( pawn.ActiveWeapon is Carriable carriable && !carriable.CanDrop ) return false;

		var weapon = PersistentEntities.GetEntity<Carriable>( WeaponID );
		if (weapon == null || weapon.Parent != null )
		{
			return false;
		}

		var pos = weapon.Position;
		if ( MathF.Abs( pos.z - Position.z ) > MAX_HEIGHT_ERROR ) return false;
		if ( pos.WithZ( Position.z ).Distance( Position ) > MAX_ERROR ) return false;

		return true;
	}

}
