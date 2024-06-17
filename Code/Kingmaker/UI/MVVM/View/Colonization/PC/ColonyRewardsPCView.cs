using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyRewardsPCView : ColonyRewardsBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatButton m_CompleteButton;

	[SerializeField]
	private TextMeshProUGUI m_CompleteButtonLabel;

	[SerializeField]
	private LootSlotPCView m_SlotPrefab;

	protected override void InitializeImpl()
	{
		m_SlotsGroup.Initialize(m_SlotPrefab);
		m_CompleteButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CompleteButton.OnLeftClickAsObservable().Subscribe(base.HandleComplete));
	}
}
