using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.TwitchDrops;

public class TwitchDropsRewardsPCView : TwitchDropsRewardsBaseView
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
		AddDisposable(base.ViewModel.IsAwaiting.Subscribe(delegate(bool isAwaiting)
		{
			m_CompleteButton.gameObject.SetActive(!isAwaiting);
		}));
		AddDisposable(m_CompleteButton.OnLeftClickAsObservable().Subscribe(base.HandleComplete));
	}
}
