using Kingmaker.Code.UI.MVVM.VM.DlcManager;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Base;

public class DlcManagerBaseView : ViewBase<DlcManagerVM>
{
	[Header("Common")]
	[SerializeField]
	protected DlcManagerMenuSelectorBaseView m_Selector;

	private void Initialize()
	{
		base.gameObject.SetActive(value: false);
		InitializeImpl();
		m_Selector.Initialize(base.ViewModel.InGame, base.ViewModel.IsConsole);
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		base.gameObject.SetActive(value: true);
		m_Selector.Bind(base.ViewModel.MenuSelectionGroup);
		Show();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		base.gameObject.SetActive(value: false);
	}

	private void Show()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.DlcModManager);
		});
		UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.DlcModManager);
		});
		UISounds.Instance.Sounds.LocalMap.MapClose.Play();
	}
}
