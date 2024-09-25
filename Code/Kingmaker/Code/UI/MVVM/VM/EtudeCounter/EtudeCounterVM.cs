using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.EtudeCounter;

public class EtudeCounterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEtudeCounterHandler, ISubscriber
{
	private class EtudeCounterConfig
	{
		public string Label;

		public Func<int> ValueGetter;

		public Func<int> TargetValueGetter;
	}

	public readonly StringReactiveProperty CounterText = new StringReactiveProperty();

	private Dictionary<string, EtudeCounterConfig> m_Configs = new Dictionary<string, EtudeCounterConfig>();

	private StringBuilder m_StringBuilder = new StringBuilder();

	private IDisposable m_UpdateSubscription;

	public EtudeCounterVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		CounterText.Value = null;
		m_UpdateSubscription?.Dispose();
		m_UpdateSubscription = null;
	}

	void IEtudeCounterHandler.ShowEtudeCounter(string id, string label, Func<int> valueGetter, Func<int> targetValueGetter)
	{
		m_Configs.Add(id, new EtudeCounterConfig
		{
			Label = label,
			ValueGetter = valueGetter,
			TargetValueGetter = targetValueGetter
		});
		if (m_UpdateSubscription == null && !m_Configs.Empty())
		{
			m_UpdateSubscription = MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(UpdateValues);
		}
	}

	void IEtudeCounterHandler.HideEtudeCounter(string id)
	{
		m_Configs.Remove(id);
		if (m_Configs.Empty() && m_UpdateSubscription != null)
		{
			m_UpdateSubscription.Dispose();
			m_UpdateSubscription = null;
			CounterText.Value = null;
		}
	}

	private void UpdateValues()
	{
		m_StringBuilder.Clear();
		foreach (EtudeCounterConfig value in m_Configs.Values)
		{
			if (m_StringBuilder.Length > 0)
			{
				m_StringBuilder.Append('\n');
			}
			m_StringBuilder.Append(value.Label);
			m_StringBuilder.Append(' ');
			m_StringBuilder.Append(value.ValueGetter());
		}
		CounterText.Value = m_StringBuilder.ToString();
	}
}
