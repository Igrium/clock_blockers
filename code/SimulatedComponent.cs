﻿#nullable enable

using Sandbox;

namespace ClockBlockers;

/// <summary>
/// Great for expanding player functionalities
/// </summary>
public class SimulatedComponent : EntityComponent<PlayerAgent>
{
	public virtual void Simulate( IClient? cl )
	{

	}
	public virtual void FrameSimulate( IClient cl )
	{

	}
	public virtual void BuildInput()
	{

	}
}
