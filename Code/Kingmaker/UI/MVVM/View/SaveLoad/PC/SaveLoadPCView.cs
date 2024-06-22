using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SaveLoad.PC;

public class SaveLoadPCView : SaveLoadBaseView
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClose();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, base.SelectPrev));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, base.SelectNext));
	}
}
