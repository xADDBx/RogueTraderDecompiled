using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.UI.MVVM.View.TwitchDrops;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootContextPCView : ViewBase<LootContextVM>
{
	[SerializeField]
	private LootPCView m_LootPCView;

	[SerializeField]
	private CargoRewardsPCView m_CargoPCView;

	[SerializeField]
	private TwitchDropsRewardsPCView m_TwitchDropsRewardsPCView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_LootPCView.Initialize();
			m_CargoPCView.Initialize();
			m_TwitchDropsRewardsPCView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LootVM.Subscribe(m_LootPCView.Bind));
		AddDisposable(base.ViewModel.CargoVM.Subscribe(m_CargoPCView.Bind));
		AddDisposable(base.ViewModel.TwitchDropsRewardsVM.Subscribe(m_TwitchDropsRewardsPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
