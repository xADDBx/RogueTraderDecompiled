using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.WarningNotification;

public class WarningsTextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IWarningNotificationUIHandler, ISubscriber, IEtudeCounterHandler
{
	public readonly ReactiveProperty<string> ShowString = new ReactiveProperty<string>();

	public readonly ReactiveProperty<WarningNotificationFormat> ShowFormat = new ReactiveProperty<WarningNotificationFormat>(WarningNotificationFormat.Common);

	public readonly ReactiveProperty<bool> IsEtudeCounterShowing = new ReactiveProperty<bool>(initialValue: false);

	public bool ShowWithSound;

	private readonly List<string> m_EtudeCounters = new List<string>();

	public WarningsTextVM()
	{
		EventBus.Subscribe(this);
	}

	protected override void DisposeImplementation()
	{
		EventBus.Unsubscribe(this);
		m_EtudeCounters.Clear();
	}

	public void HandleWarning(WarningNotificationType warningType, bool addToLog, WarningNotificationFormat warningFormat, bool withSound = true)
	{
		ShowWithSound = withSound;
		ShowFormat.Value = warningFormat;
		ShowString.SetValueAndForceNotify(LocalizedTexts.Instance.WarningNotification.GetText(warningType));
	}

	public void HandleWarning(string str, bool addToLog, WarningNotificationFormat warningFormat, bool withSound = true)
	{
		ShowWithSound = withSound;
		ShowFormat.Value = warningFormat;
		ShowString.SetValueAndForceNotify(str);
	}

	public void ShowEtudeCounter(EtudeShowCounterUIStruct counterUIStruct)
	{
		m_EtudeCounters.Add(counterUIStruct.Id);
		IsEtudeCounterShowing.Value = m_EtudeCounters.Count > 0;
	}

	public void HideEtudeCounter(string id)
	{
		m_EtudeCounters.Remove(id);
		IsEtudeCounterShowing.Value = m_EtudeCounters.Count > 0;
	}
}
