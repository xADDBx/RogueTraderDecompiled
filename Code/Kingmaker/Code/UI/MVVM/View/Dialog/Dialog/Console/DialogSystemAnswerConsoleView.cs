using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;

public class DialogSystemAnswerConsoleView : DialogSystemAnswerBaseView, IConfirmClickHandler, IConsoleEntity
{
	[SerializeField]
	private Image m_Image;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Image.sprite = ConsoleRoot.Instance.Icons.GetIcon(RewiredActionType.Confirm);
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_Image.gameObject.SetActive(value);
	}

	public bool CanConfirmClick()
	{
		return m_Image.IsActive();
	}

	public void OnConfirmClick()
	{
		base.ViewModel.OnChooseAnswer();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void ShowAnswerHint(bool value)
	{
		m_Image.gameObject.SetActive(value);
	}
}
