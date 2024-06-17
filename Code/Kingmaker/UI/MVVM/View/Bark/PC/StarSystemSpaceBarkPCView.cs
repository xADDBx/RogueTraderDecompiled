using Kingmaker.UI.MVVM.View.Bark.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Bark.PC;

public class StarSystemSpaceBarkPCView : StarSystemSpaceBarkBaseView
{
	[SerializeField]
	private OwlcatButton m_ShowLastMessageButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ShowLastMessageButton.OnLeftClickAsObservable().Subscribe(base.ShowLastMessage));
	}

	protected override void ShowAnimatedImpl()
	{
		m_ShowLastMessageButton.SetInteractable(state: false);
	}

	protected override void ResetAnimationImpl()
	{
		m_ShowLastMessageButton.SetInteractable(state: true);
	}
}
