using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;
using Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow;

public class AllSystemsInformationWindowPCView : AllSystemsInformationWindowBaseView
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private SystemInfoAllSystemsInformationWindowPCView m_SystemInfoAllSystemsInformationWindowPCViewPrefab;

	private bool m_EscIsSubscribed;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.CloseInformationWindow));
	}

	protected override void DestroyViewImplementation()
	{
		if (m_EscIsSubscribed)
		{
			EscHotkeyManager.Instance.Unsubscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = false;
		}
		base.DestroyViewImplementation();
	}

	protected override void ShowHideWindow(bool state)
	{
		if (!state)
		{
			EscHotkeyManager.Instance.Unsubscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = false;
		}
		base.ShowHideWindow(state);
		if (state)
		{
			EscHotkeyManager.Instance.Subscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = true;
		}
	}

	protected override void DrawSystems()
	{
		base.DrawSystems();
		SystemInfoAllSystemsInformationWindowVM[] array = base.ViewModel.Systems.ToArray();
		if (array.Any())
		{
			m_SystemsWidgetList.DrawEntries(array, m_SystemInfoAllSystemsInformationWindowPCViewPrefab);
		}
	}
}
