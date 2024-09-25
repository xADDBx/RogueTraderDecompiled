using System;
using Code.Package.Runtime.Extensions.Dependencies;
using UniRx;
using UniRx.Operators;

namespace Owlcat.Runtime.UniRx;

internal class TryCatchFinallyObservable<TSource, TException> : OperatorObservableBase<TSource> where TException : Exception
{
	private class TryCatchFinallyObserver : OperatorObserverBase<TSource, TSource>
	{
		private TryCatchFinallyObservable<TSource, TException> m_Parent;

		public TryCatchFinallyObserver(TryCatchFinallyObservable<TSource, TException> parent, IObserver<TSource> observer, IDisposable cancel)
			: base(observer, cancel)
		{
			m_Parent = parent;
		}

		public override void OnNext(TSource value)
		{
			try
			{
				observer.OnNext(value);
			}
			catch (TException obj)
			{
				m_Parent.m_CatchAction?.Invoke(obj);
			}
			finally
			{
				m_Parent.m_FinallyAction?.Invoke();
			}
		}

		public override void OnError(Exception error)
		{
			try
			{
				observer.OnError(error);
			}
			finally
			{
				Dispose();
			}
		}

		public override void OnCompleted()
		{
			try
			{
				observer.OnCompleted();
			}
			finally
			{
				Dispose();
			}
		}
	}

	private readonly IObservable<TSource> m_Source;

	private readonly Action<TException> m_CatchAction;

	private readonly Action m_FinallyAction;

	internal TryCatchFinallyObservable(IObservable<TSource> source, Action<TException> catchAction, Action finallyAction)
		: base(source.IsRequiredSubscribeOnCurrentThread())
	{
		m_Source = source;
		m_CatchAction = catchAction ?? new Action<TException>(DefaultCatchAction);
		m_FinallyAction = finallyAction;
	}

	protected override IDisposable SubscribeCore(IObserver<TSource> observer, IDisposable cancel)
	{
		return m_Source.Subscribe(new TryCatchFinallyObserver(this, observer, cancel));
	}

	private static void DefaultCatchAction(Exception e)
	{
		UniRxLogger.Exception("Error in observable: " + e);
	}
}
