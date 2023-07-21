using Sandbox;

namespace ClockBlockers;

/// <summary>
/// Component designed for animation stuff, only 1 per pawn.
/// </summary>
public class AnimationComponent : EntityComponent<Player>, ISingletonComponent
{

	public virtual void Simulate( IClient cl )
	{

	}
	public virtual void FrameSimulate( IClient cl )
	{

	}
	public virtual void BuildInput()
	{

	}
}
