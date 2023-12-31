﻿using Sandbox;
namespace ClockBlockers;

public partial class PlayerAgent
{
	public ClothingContainer Clothing { get; protected set; } = new();

	/// <summary>
	/// Set the clothes to whatever the player is wearing
	/// </summary>
	public void UpdateClothes( IClient cl )
	{
		Clothing ??= new();
		Clothing.LoadFromClient( cl );
	}
}
