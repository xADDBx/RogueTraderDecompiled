using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace Owlcat.Runtime.UniRx;

internal class SelectReactiveCollection<TSource, TResult> : IReadOnlyReactiveCollection<TResult>, IEnumerable<TResult>, IEnumerable
{
	private IReadOnlyReactiveCollection<TSource> m_SourceCollection;

	private Func<TSource, TResult> m_Selector;

	public int Count => m_SourceCollection.Count;

	public TResult this[int index] => m_Selector(m_SourceCollection[index]);

	public SelectReactiveCollection(IReadOnlyReactiveCollection<TSource> sourceCollection, Func<TSource, TResult> selector)
	{
		m_SourceCollection = sourceCollection;
		m_Selector = selector;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<TResult> GetEnumerator()
	{
		return new SelectEnumerator<TSource, TResult>(m_SourceCollection.GetEnumerator(), m_Selector);
	}

	public IObservable<CollectionAddEvent<TResult>> ObserveAdd()
	{
		return from evt in m_SourceCollection.ObserveAdd()
			select new CollectionAddEvent<TResult>(evt.Index, m_Selector(evt.Value));
	}

	public IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false)
	{
		return m_SourceCollection.ObserveCountChanged();
	}

	public IObservable<CollectionMoveEvent<TResult>> ObserveMove()
	{
		return from evt in m_SourceCollection.ObserveMove()
			select new CollectionMoveEvent<TResult>(evt.OldIndex, evt.NewIndex, m_Selector(evt.Value));
	}

	public IObservable<CollectionRemoveEvent<TResult>> ObserveRemove()
	{
		return from evt in m_SourceCollection.ObserveRemove()
			select new CollectionRemoveEvent<TResult>(evt.Index, m_Selector(evt.Value));
	}

	public IObservable<CollectionReplaceEvent<TResult>> ObserveReplace()
	{
		return from evt in m_SourceCollection.ObserveReplace()
			select new CollectionReplaceEvent<TResult>(evt.Index, m_Selector(evt.OldValue), m_Selector(evt.NewValue));
	}

	public IObservable<Unit> ObserveReset()
	{
		return m_SourceCollection.ObserveReset();
	}
}
