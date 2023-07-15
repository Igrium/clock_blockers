#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ClockBlockers.Anim;

/// <summary>
/// An animation that can be played back on a player pawn
/// </summary>
public class Animation
{

	/// <summary>
	/// The tick rate at which this animation was recorded.
	/// </summary>
	public int TickRate { get; set; }

	/// <summary>
	/// All the segments in this animation. Each segment is one second.
	/// </summary>
	public IList<AnimSegment> Segments { get; } = new List<AnimSegment>();

	public ClothingContainer? Clothing { get; set; } = new();

	public bool IsEmpty => Segments.Count == 0 || Segments[0].Count == 0;

	/// <summary>
	/// Get the correct segment for a given timecode.
	/// </summary>
	/// <param name="time">Seconds since the beginning of the animation.</param>
	/// <returns>The segment.</returns>
	/// <exception cref="ArgumentOutOfRangeException">If time < 0</exception>
	public AnimSegment GetSegment( float time )
	{
		return Segments[GetSegmentIndex( time )];
	}

	public int GetSegmentIndex( float time )
	{
		if ( time < 0 )
		{
			throw new ArgumentOutOfRangeException( "time" );
		}

		int index = TimestampToSegment( time );
		if ( index >= Segments.Count ) index = Segments.Count - 1;
		return index;
	}

	public static int TimestampToSegment( float time )
	{
		return (int)MathF.Floor( time );
	}

	public float Length
	{
		get
		{
			int numSegments = Segments.Count;
			if ( numSegments <= 0 ) return 0;
			float segmentTime = numSegments - 1;

			var lastSegment = Segments[numSegments - 1];
			float preciseTime = lastSegment.Frames.Count() / TickRate;

			return segmentTime + preciseTime;
		}
	}
}
