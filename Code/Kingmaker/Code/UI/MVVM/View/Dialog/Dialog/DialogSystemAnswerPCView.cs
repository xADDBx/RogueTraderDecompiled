using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogSystemAnswerPCView : DialogSystemAnswerBaseView
{
	public new void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(Game.Instance.Keyboard.Bind("NextOrEnd", delegate
		{
			base.ViewModel.OnChooseAnswer();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind("DialogChoice1", delegate
		{
			base.ViewModel.OnChooseAnswer();
		}));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnChooseAnswer();
		}));
	}
}
