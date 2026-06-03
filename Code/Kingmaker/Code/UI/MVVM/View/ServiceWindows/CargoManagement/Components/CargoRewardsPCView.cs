using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoRewardsPCView : CargoRewardsBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatButton m_CompleteButton;

	[SerializeField]
	private TextMeshProUGUI m_CompleteButtonLabel;

	protected override void InitializeImpl()
	{
		string text = UIStrings.Instance.CommonTexts.Accept.Text;
		if (!ApplicationHelper.IsRunningOnSwitch2 || !SettingsRoot.Game.Switch.SwitchJoyConAsMouse)
		{
			text = text + " [" + UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("CollectAllLoot")) + "]";
		}
		m_CompleteButtonLabel.text = text;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CompleteButton.OnLeftClickAsObservable().Subscribe(base.HandleComplete));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.CollectAllLoot.name, base.HandleComplete));
	}
}
