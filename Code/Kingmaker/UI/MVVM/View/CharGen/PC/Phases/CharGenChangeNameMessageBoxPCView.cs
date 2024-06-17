using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.MessageBox.PC;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.CharGen;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC.Phases;

public class CharGenChangeNameMessageBoxPCView : MessageBoxPCView
{
	[Header("CharGenChangeNameMessageBoxPCView")]
	[SerializeField]
	private OwlcatButton m_RandomNameButton;

	[SerializeField]
	private TextMeshProUGUI m_RandomNameLabel;

	private CharGenChangeNameMessageBoxVM ChangeNameViewModel => base.ViewModel as CharGenChangeNameMessageBoxVM;

	public override void Initialize()
	{
		base.Initialize();
		m_RandomNameLabel.text = UIStrings.Instance.CharGen.SetRandomNameButton;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_RandomNameButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ChangeNameViewModel.SetRandomName();
		}));
		AddDisposable(m_RandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName));
	}

	protected override void OnTextInputChanged(string value)
	{
		string text = "";
		if (value.EndsWith(" "))
		{
			text = " ";
		}
		value = value.Trim();
		value += text;
		m_InputField.text = value;
		base.ViewModel.InputText.Value = value;
	}
}
