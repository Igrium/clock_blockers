#nullable enable

using Sandbox;

namespace ClockBlockers.AI;


/// <summary>
/// The different ways an agent can move.
/// </summary>
public enum MovementType
{
	Standard,
	Sprint,
	Walk,
	Duck
}

public interface IPathTarget
{
	public MovementType MoveType { get; }
	public Vector3 CurrentPosition();
	public float TargetEpsilon { get; }
}

public struct VectorPathTarget : IPathTarget
{
	public MovementType MoveType { get; set; }
	public Vector3 Target { get; set; }
	public float TargetEpsilon { get; set; } = 16f;

	public VectorPathTarget(Vector3 target)
	{
		this.Target = target;
	}

	public Vector3 CurrentPosition()
	{
		return Target;
	}
}

public struct EntityPathTarget : IPathTarget
{
	public MovementType MoveType { get; set; }
	public Entity Target { get; set; }
	public float TargetEpsilon { get; set; } = 32f;

	public EntityPathTarget(Entity target) {
		this.Target = target;
	}

	public Vector3 CurrentPosition()
	{
		return Target.IsValid() ? Target.Position : Vector3.Zero;
	}
}
