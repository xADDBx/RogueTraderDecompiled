using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Kingmaker.Utility.BudgetedUpdate;

public abstract class BudgetedUpdateRoot : MonoBehaviour, IObservable<Unit>
{
	private readonly struct Unsubscriber : IDisposable
	{
		private readonly BudgetedUpdateRoot m_Owner;

		private readonly IObserver<Unit> m_Observer;

		public Unsubscriber(BudgetedUpdateRoot owner, IObserver<Unit> observer)
		{
			m_Owner = owner;
			m_Observer = observer;
		}

		public void Dispose()
		{
			if ((bool)m_Owner)
			{
				m_Owner.RemoveObserver(m_Observer);
			}
		}
	}

	private readonly TimeSpan m_Budget;

	private readonly LinkedList<IObserver<Unit>> m_Observers = new LinkedList<IObserver<Unit>>();

	private LinkedListNode<IObserver<Unit>> m_CurrentObserver;

	private readonly IEnumerator<IObserver<Unit>> m_Enumerator;

	private DateTime m_LastUpdateTimestamp = DateTime.Now;

	protected BudgetedUpdateRoot(TimeSpan budget)
	{
		m_Budget = budget;
		m_Enumerator = UpdateInternal();
	}

	protected void Invoke()
	{
		if (m_Observers.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		TimeSpan timeSpan = now - m_LastUpdateTimestamp;
		m_LastUpdateTimestamp = now;
		long num = m_Observers.Count * timeSpan.Ticks;
		TimeSpan budget = m_Budget;
		long num2 = Math.Clamp(num / budget.Ticks, 1L, m_Observers.Count);
		for (long num3 = 0L; num3 < num2; num3++)
		{
			m_Enumerator.MoveNext();
			try
			{
				m_Enumerator.Current.OnNext(Unit.Default);
			}
			catch (Exception ex)
			{
				PFLog.System.Error(ex);
			}
		}
	}

	protected abstract void Destroy();

	private IEnumerator<IObserver<Unit>> UpdateInternal()
	{
		while (true)
		{
			m_CurrentObserver = m_CurrentObserver?.Next ?? m_Observers.First;
			yield return m_CurrentObserver?.Value;
		}
	}

	public void AddObserver(IObserver<Unit> observer)
	{
		m_Observers.AddLast(observer);
	}

	public void RemoveObserver(IObserver<Unit> observer)
	{
		LinkedListNode<IObserver<Unit>> linkedListNode = m_Observers.Find(observer);
		if (linkedListNode != null)
		{
			if (m_CurrentObserver == linkedListNode)
			{
				m_CurrentObserver = linkedListNode.Next;
			}
			m_Observers.Remove(linkedListNode);
		}
		if (m_Observers.Count == 0)
		{
			Destroy();
		}
	}

	public IDisposable Subscribe(IObserver<Unit> observer)
	{
		AddObserver(observer);
		return new Unsubscriber(this, observer);
	}
}
