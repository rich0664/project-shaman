using System.Collections.Generic;
using System.Linq;

public static class Extentions {
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
	{
		return new HashSet<T>(source);
	}
}
