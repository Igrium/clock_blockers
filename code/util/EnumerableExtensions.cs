using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Util;
public static class EnumerableExtensions
{
	/// <summary>
	/// Makes an enumerable loop on itself.
	/// </summary>
	/// <returns>Looping enumerable.</returns>
	public static IEnumerable<T> Loop<T>(this IEnumerable<T> source)
	{
		while (true)
		{
			foreach (var item in source)
			{
				yield return item;
			}
		}
	}

	public static T RandomElement<T> (this IEnumerable<T> source)
	{
		return source.RandomElement( new Random() );
	}

	public static T RandomElement<T> (this IEnumerable<T> source, Random rand)
	{
		int index = rand.Next( 0, source.Count() );
		return source.ElementAt( index );
	}
}
