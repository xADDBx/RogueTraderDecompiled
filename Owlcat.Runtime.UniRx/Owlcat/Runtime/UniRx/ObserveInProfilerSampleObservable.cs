using System;
using UniRx;
using UniRx.Operators;

namespace Owlcat.Runtime.UniRx;

internal class ObserveInProfilerSampleObservable<TSource> : OperatorObservableBase<TSource>
{
	private class ObserveInProfilerSampleObserver : OperatorObserverBase<TSource, TSource>
	{
		private ObserveInProfilerSampleObservable<TSource> m_Parent;

		private IObserver<TSource> m_Observer;

		public ObserveInProfilerSampleObserver(ObserveInProfilerSampleObservable<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
			: base(observer, cancel)
		{
			m_Parent = parent;
			m_Observer = observer;
		}

		public override void OnNext(TSource value)
		{
			m_Observer.OnNext(value);
		}

		public override void OnCompleted()
		{
			m_Observer.OnCompleted();
		}

		public override void OnError(Exception error)
		{
			m_Observer.OnError(error);
		}
	}

	private IObservable<TSource> m_Source;

	private string m_SampleName;

	internal ObserveInProfilerSampleObservable(IObservable<TSource> source, string sampleName)
		: base(source.IsRequiredSubscribeOnCurrentThread())
	{
		m_Source = source;
		m_SampleName = sampleName;
	}

	protected override IDisposable SubscribeCore(IObserver<TSource> observer, IDisposable cancel)
	{
		return m_Source.Subscribe(new ObserveInProfilerSampleObserver(this, observer, cancel));
	}
}
