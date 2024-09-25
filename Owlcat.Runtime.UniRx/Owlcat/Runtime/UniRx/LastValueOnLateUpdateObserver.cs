using System;
using UniRx.Operators;

namespace Owlcat.Runtime.UniRx;

internal class LastValueOnLateUpdateObserver<TSource> : OperatorObserverBase<TSource, TSource>, ICanLateUpdate
{
	private TSource m_LastValue;

	private bool m_ReceivedValueInLastFrame;

	internal LastValueOnLateUpdateObserver(IObserver<TSource> observer, IDisposable cancel)
		: base(observer, cancel)
	{
	}

	public void OnLateUpdate()
	{
		if (m_ReceivedValueInLastFrame)
		{
			observer.OnNext(m_LastValue);
		}
		m_ReceivedValueInLastFrame = false;
	}

	public override void OnNext(TSource value)
	{
		m_LastValue = value;
		m_ReceivedValueInLastFrame = true;
	}

	public override void OnCompleted()
	{
		observer.OnCompleted();
	}

	public override void OnError(Exception error)
	{
		observer.OnError(error);
	}
}
