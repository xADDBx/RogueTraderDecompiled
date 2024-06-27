using System;
using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public static class EqualitySelector
{
	private class EqualityImpl<TSource, TTarget> : IEqualityComparer<TSource>
	{
		private readonly Func<TSource, TTarget> m_Selector;

		public EqualityImpl(Func<TSource, TTarget> selector)
		{
			m_Selector = selector;
		}

		public bool Equals(TSource x, TSource y)
		{
			return EqualityComparer<TTarget>.Default.Equals(m_Selector(x), m_Selector(y));
		}

		public int GetHashCode(TSource obj)
		{
			return EqualityComparer<TTarget>.Default.GetHashCode(m_Selector(obj));
		}
	}

	public static IEqualityComparer<TSource> Create<TSource, TTarget>(Func<TSource, TTarget> selector)
	{
		return new EqualityImpl<TSource, TTarget>(selector);
	}
}
