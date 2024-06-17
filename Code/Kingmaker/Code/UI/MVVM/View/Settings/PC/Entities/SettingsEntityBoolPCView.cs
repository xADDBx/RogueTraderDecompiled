using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntityBoolPCView : SettingsEntityWithValueView<SettingsEntityBoolVM>
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.TempValue.Subscribe(SetValueFromSettings));
		AddDisposable(m_MultiButton.OnLeftClickAsObservable().Subscribe(SwitchValue));
		SubscribeNotAllowedSelectable(m_MultiButton);
		SetToggleTexts();
	}

	protected override void UpdateLocalization()
	{
		base.UpdateLocalization();
		SetToggleTexts();
	}

	private void SetToggleTexts()
	{
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
	}

	private void SwitchValue()
	{
		base.ViewModel.SetTempValue(!base.ViewModel.GetTempValue());
	}

	private void SetValueFromSettings(bool value)
	{
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_MultiButton.Interactable = allowed;
		SetNotAllowedModificationHint(m_MultiButton);
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			base.ViewModel.ChangeValue();
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			base.ViewModel.ChangeValue();
		}
		return true;
	}
}
