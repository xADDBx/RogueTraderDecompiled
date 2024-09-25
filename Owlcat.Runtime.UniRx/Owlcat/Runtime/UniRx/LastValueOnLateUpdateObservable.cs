using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Operators;

namespace Owlcat.Runtime.UniRx;

internal class LastValueOnLateUpdateObservable<TSource> : OperatorObservableBase<TSource>, ICanLateUpdate
{
	private readonly IObservable<TSource> m_Source;

	private readonly LinkedList<ICanLateUpdate> m_Observers = new LinkedList<ICanLateUpdate>();

	private readonly LinkedListNode<ICanLateUpdate> m_Node;

	internal LastValueOnLateUpdateObservable(IObservable<TSource> source)
		: base(source.IsRequiredSubscribeOnCurrentThread())
	{
		m_Source = source;
		m_Node = new LinkedListNode<ICanLateUpdate>(this);
	}

	public void OnLateUpdate()
	{
		foreach (ICanLateUpdate observer in m_Observers)
		{
			observer.OnLateUpdate();
		}
	}

	protected override IDisposable SubscribeCore(IObserver<TSource> observer, IDisposable cancel)
	{
		LastValueOnLateUpdateObserver<TSource> lateUpdateObserver = new LastValueOnLateUpdateObserver<TSource>(observer, cancel);
		if (m_Observers.Count == 0)
		{
			LastValueOnLateUpdateObservableFabric.Instance.Add(m_Node);
		}
		m_Observers.AddLast(lateUpdateObserver);
		return new CompositeDisposable(m_Source.Subscribe(lateUpdateObserver), Disposable.Create(delegate
		{
			m_Observers.Remove(lateUpdateObserver);
			if (m_Observers.Count == 0)
			{
				LastValueOnLateUpdateObservableFabric.Instance.Remove(m_Node);
			}
		}));
	}
}
