using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.PC;

public class StatCheckLootUnitCardPCView : StatCheckLootUnitCardBaseView
{
	[SerializeField]
	private OwlcatButton m_CheckStatButton;

	[SerializeField]
	private TextMeshProUGUI m_CheckStatButtonLabel;

	[SerializeField]
	private OwlcatButton m_SwitchUnitButton;

	protected override void InitializeImpl()
	{
		m_CheckStatButtonLabel.text = UIStrings.Instance.ExplorationTexts.StatCheckLootCheckStatButton;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CheckStatButton.OnLeftClickAsObservable().Subscribe(base.OnCheckStat));
		AddDisposable(m_SwitchUnitButton.OnLeftClickAsObservable().Subscribe(base.OnSwitchUnit));
		AddDisposable(m_CheckStatButton.OnConfirmClickAsObservable().Subscribe(base.OnCheckStat));
		AddDisposable(m_SwitchUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnSwitchUnit));
	}
}
