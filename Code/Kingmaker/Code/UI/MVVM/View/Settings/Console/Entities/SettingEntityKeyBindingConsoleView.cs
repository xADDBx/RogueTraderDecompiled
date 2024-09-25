using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingEntityKeyBindingConsoleView : SettingsEntityWithValueConsoleView<SettingEntityKeyBindingVM>
{
	[SerializeField]
	private OwlcatMultiButton m_BindingButton1;

	[SerializeField]
	private OwlcatMultiButton m_BindingButton2;

	[SerializeField]
	private TextMeshProUGUI m_BindingText1;

	[SerializeField]
	private TextMeshProUGUI m_BindingText2;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_BindingButton1.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenBindingDialogVM(0);
		}));
		AddDisposable(m_BindingButton2.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenBindingDialogVM(1);
		}));
	}

	private void SetupBindingButton(OwlcatMultiButton button, TextMeshProUGUI buttonText, string text)
	{
		bool flag = string.IsNullOrEmpty(text);
		button.SetActiveLayer(flag ? "Off" : "On");
		buttonText.text = (flag ? "---" : text);
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_BindingButton1.Interactable = allowed;
		m_BindingButton2.Interactable = allowed;
	}

	public override bool HandleLeft()
	{
		_ = base.ViewModel.ModificationAllowed.Value;
		return false;
	}

	public override bool HandleRight()
	{
		_ = base.ViewModel.ModificationAllowed.Value;
		return false;
	}
}
