using System;
using System.Collections;
using System.Collections.Generic;

namespace Owlcat.Runtime.UniRx;

internal class SelectEnumerator<TSource, TResult> : IEnumerator<TResult>, IEnumerator, IDisposable
{
	private IEnumerator<TSource> m_SourceCollection;

	private Func<TSource, TResult> m_Selector;

	object IEnumerator.Current => Current;

	public TResult Current => m_Selector(m_SourceCollection.Current);

	public SelectEnumerator(IEnumerator<TSource> sourceCollection, Func<TSource, TResult> selector)
	{
		m_SourceCollection = sourceCollection;
		m_Selector = selector;
	}

	public bool MoveNext()
	{
		return m_SourceCollection.MoveNext();
	}

	public void Reset()
	{
		m_SourceCollection.Reset();
	}

	public void Dispose()
	{
		m_SourceCollection.Dispose();
	}
}
