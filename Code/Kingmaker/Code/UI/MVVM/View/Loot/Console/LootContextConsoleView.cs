using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.View.TwitchDrops;
using Kingmaker.UI.MVVM.VM.TwitchDrops;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootContextConsoleView : ViewBase<LootContextVM>
{
	[SerializeField]
	private UIDestroyViewLink<LootConsoleView, LootVM> m_LootConsoleViewLink;

	[SerializeField]
	private UIDestroyViewLink<CargoRewardsConsoleView, CargoRewardsVM> m_CargoConsoleViewLink;

	[SerializeField]
	private UIDestroyViewLink<TwitchDropsRewardsConsoleView, TwitchDropsRewardsVM> m_TwitchDropsConsoleViewLink;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LootVM.Subscribe(m_LootConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.CargoVM.Subscribe(m_CargoConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.TwitchDropsRewardsVM.Subscribe(m_TwitchDropsConsoleViewLink.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
