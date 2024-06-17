using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.PC;

public class StatCheckLootUnitsPagePCView : StatCheckLootUnitsPageBaseView<StatCheckLootUnitCardPCView, StatCheckLootSmallUnitCardPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_BackWithoutConfirmUnitButton;

	[SerializeField]
	private OwlcatButton m_ConfirmUnitButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmUnitButtonLabel;

	protected override void InitializeImpl()
	{
		m_ConfirmUnitButtonLabel.text = UIStrings.Instance.ExplorationTexts.StatCheckLootConfirmSelectedUnitButton;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_BackWithoutConfirmUnitButton.OnLeftClickAsObservable().Subscribe(base.OnBackWithoutConfirmUnit));
		AddDisposable(m_ConfirmUnitButton.OnLeftClickAsObservable().Subscribe(base.OnConfirmUnit));
		AddDisposable(m_BackWithoutConfirmUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnBackWithoutConfirmUnit));
		AddDisposable(m_ConfirmUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnConfirmUnit));
	}
}
