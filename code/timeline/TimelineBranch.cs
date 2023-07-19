#nullable enable

using ClockBlockers.Anim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClockBlockers.Timeline;

/// <summary>
/// Represents a branch of an agent's timeline.
/// A branch contains the animation data between two events.
/// It then references two other branches, which it chooses
/// based on the result of its ending event.
/// </summary>
public class TimelineBranch
{
	/// <summary>
	/// The event that concludes this branch
	/// </summary>
	public ITimelineEvent? EndEvent { get; set; }

	/// <summary>
	/// The timestamp relative to the start of the animation at which the end event will be tested.
	/// </summary>
	public float EndEventTime { get; set; }

	/// <summary>
	/// The branch the remnant will take if the end event is successful.
	/// </summary>
	public TimelineBranch? BranchA { get; set; }

	/// <summary>
	/// The branch the remnant will take if the end event fails.
	/// </summary>
	public TimelineBranch? BranchB { get; set; }

	/// <summary>
	/// The animation for this timeline to play.
	/// </summary>
	public Animation Animation { get; private set; }

	/// <summary>
	/// The persistent ID of the entity this timeline belongs to.
	/// </summary>
	public string? PersistentID { get; set; }

	/// <summary>
	/// If set, the remnant will recieve this weapon when this timeline is played,
	/// assuming it doesn't already have a weapon.
	/// </summary>
	public WeaponSpawner? Weapon { get; set; }

	public TimelineBranch( Animation animation )
	{
		this.Animation = animation;
	}
}
