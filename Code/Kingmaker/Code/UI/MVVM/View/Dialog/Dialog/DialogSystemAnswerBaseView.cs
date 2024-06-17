using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogSystemAnswerBaseView : ViewBase<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	protected OwlcatButton m_Button;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.Answer.Subscribe(delegate(BlueprintAnswer value)
		{
			m_Text.text = value.Text;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
