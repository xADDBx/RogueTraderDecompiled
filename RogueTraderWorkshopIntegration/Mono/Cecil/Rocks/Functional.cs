using System;
using System.Collections.Generic;

namespace Mono.Cecil.Rocks;

internal static class Functional
{
	public static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f)
	{
		Func<A, R> g = null;
		g = f((A a) => g(a));
		return g;
	}

	public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return PrependIterator(source, element);
	}

	private static IEnumerable<TSource> PrependIterator<TSource>(IEnumerable<TSource> source, TSource element)
	{
		yield return element;
		foreach (TSource item in source)
		{
			yield return item;
		}
	}
}
