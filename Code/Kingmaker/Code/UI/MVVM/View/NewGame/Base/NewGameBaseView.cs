using Kingmaker.Code.UI.MVVM.VM.NewGame;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGameBaseView : ViewBase<NewGameVM>
{
	[Header("Common")]
	[SerializeField]
	protected NewGameMenuSelectorBaseView m_Selector;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_Selector.Bind(base.ViewModel.MenuSelectionGroup);
		Show();
		AddDisposable(base.ViewModel.IsActive.Subscribe(base.gameObject.SetActive));
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
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.NewGame);
		});
		UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.NewGame);
		});
		UISounds.Instance.Sounds.LocalMap.MapClose.Play();
	}

	protected void Close()
	{
		SettingsController.Instance.RevertAllTempValues();
		base.ViewModel.OnBack();
	}
}
