using System;
using UniRx;
using UniRx.Operators;

namespace Owlcat.Runtime.UniRx;

internal class DefaultIfNullObservable<T> : OperatorObservableBase<T> where T : class
{
	private class DefaultIfNull : OperatorObserverBase<T, T>
	{
		private readonly DefaultIfNullObservable<T> m_Parent;

		public DefaultIfNull(DefaultIfNullObservable<T> parent, IObserver<T> observer, IDisposable cancel)
			: base(observer, cancel)
		{
			m_Parent = parent;
		}

		public override void OnNext(T value)
		{
			observer.OnNext(value ?? m_Parent.m_DefaultValue);
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

	private readonly IObservable<T> m_Source;

	private readonly T m_DefaultValue;

	internal DefaultIfNullObservable(IObservable<T> source, T defaultValue)
		: base(source.IsRequiredSubscribeOnCurrentThread())
	{
		m_Source = source;
		m_DefaultValue = defaultValue;
	}

	protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
	{
		return m_Source.Subscribe(new DefaultIfNull(this, observer, cancel));
	}
}
