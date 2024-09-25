using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.UI.MVVM.VM.CharGen;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases;

public class CharGenChangeNameMessageBoxConsoleView : MessageBoxConsoleView
{
	private bool m_IsAcceptInteractable;

	private CharGenChangeNameMessageBoxVM ChangeNameViewModel => base.ViewModel as CharGenChangeNameMessageBoxVM;

	protected override void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		base.CreateInputImpl(inputLayer, hintsWidget);
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ChangeNameViewModel.SetRandomName();
		}, 10, base.ViewModel.IsCheckbox.Not().ToReactiveProperty()), UIStrings.Instance.CharGen.SetRandomNameButton));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			m_InputField.Select();
		}, 11, CanEditNameByYourself, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.EditNameButton));
		ConfirmBindActive.Value = true;
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		m_IsAcceptInteractable = interactable;
	}

	protected override void OnConfirmClick()
	{
		if (m_IsAcceptInteractable)
		{
			base.OnConfirmClick();
		}
		else if (CanEditNameByYourself.Value)
		{
			m_InputField.Select();
		}
	}

	protected override void OnTextInputChanged(string value)
	{
		string text = string.Empty;
		if (value.EndsWith(" "))
		{
			text = " ";
		}
		value = value.Trim();
		value += text;
		m_InputField.InputField.text = value;
		base.ViewModel.InputText.Value = value;
		m_InputField.InputField.textComponent.ForceMeshUpdate(ignoreActiveState: true);
	}
}
