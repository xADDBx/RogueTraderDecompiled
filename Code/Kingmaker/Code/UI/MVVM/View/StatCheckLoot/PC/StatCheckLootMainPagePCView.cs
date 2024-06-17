using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.PC;

public class StatCheckLootMainPagePCView : StatCheckLootMainPageBaseView<StatCheckLootUnitCardPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.OnClose));
		AddDisposable(m_CloseButton.OnConfirmClickAsObservable().Subscribe(base.OnClose));
	}
}
