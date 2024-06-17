using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootContextConsoleView : ViewBase<LootContextVM>
{
	[SerializeField]
	private LootConsoleView m_LootConsoleView;

	[SerializeField]
	private CargoRewardsConsoleView m_CargoConsoleView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_LootConsoleView.Initialize();
			m_CargoConsoleView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LootVM.Subscribe(m_LootConsoleView.Bind));
		AddDisposable(base.ViewModel.CargoVM.Subscribe(m_CargoConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
