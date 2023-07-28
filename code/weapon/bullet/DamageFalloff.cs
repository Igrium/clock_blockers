#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClockBlockers.Weapon;

public enum DamageFalloffType
{
	Constant,
	Linear,
	Exponential
}

/// <summary>
/// Methods pertaining to bullet falloff functions.
/// </summary>
public static class DamageFalloff
{
	public delegate float FalloffFunction( float baseDamage, float distance, float factor );

	public static readonly IReadOnlyDictionary<DamageFalloffType, FalloffFunction> Functions = new Dictionary<DamageFalloffType, FalloffFunction>()
	{
		{DamageFalloffType.Constant, constantFalloff},
		{DamageFalloffType.Linear, linearFalloff },
		{DamageFalloffType.Exponential, exponentialFalloff }
	};

	private static float constantFalloff( float baseDamage, float distance, float factor ) => baseDamage;

	private static float linearFalloff( float baseDamage, float distance, float factor )
		=> -(distance * factor) + baseDamage;

	private static float exponentialFalloff( float baseDamage, float distance, float factor )
		=> -MathF.Pow( factor + 1, distance ) + baseDamage;

	/// <summary>
	/// Calculate the amount of damage for a specified distance.
	/// </summary>
	/// <param name="baseDamage">The base amount of damage.</param>
	/// <param name="distance">The distance to use.</param>
	/// <param name="factor">A "multiplier" to use for the falloff function. Usage depends on function implementation.</param>
	/// <param name="falloffFunction">The falloff function to use.</param>
	/// <returns>The calculated damage.</returns>
	public static float CalculateDamage( float baseDamage, float distance, float factor, DamageFalloffType falloffFunction = DamageFalloffType.Constant )
	{
		FalloffFunction? function;
		if ( Functions.TryGetValue( falloffFunction, out function ) )
		{
			return function.Invoke( baseDamage, distance, factor );
		}
		else
		{
			throw new ArgumentException( "Unknown function type: " + falloffFunction, "falloffFunction" );
		}
	}
}
