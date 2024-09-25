using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalBaseView : ViewBase<JournalVM>, IInitializable
{
	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	private JournalResourcesBaseView m_SystemMapSpaceResourcesPCView;

	private bool m_IsShowed;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(m_PantographView.Show());
		m_SystemMapSpaceResourcesPCView.Bind(base.ViewModel.SystemMapSpaceResourcesVM);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			base.gameObject.SetActive(value: true);
			UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Journal);
			});
		}
	}

	private void Hide()
	{
		if (m_IsShowed)
		{
			m_IsShowed = false;
			base.gameObject.SetActive(value: false);
			UISounds.Instance.Sounds.LocalMap.MapClose.Play();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Journal);
			});
		}
	}
}
