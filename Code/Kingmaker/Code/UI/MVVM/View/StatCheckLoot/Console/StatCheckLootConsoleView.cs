using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Console;

public class StatCheckLootConsoleView : StatCheckLootBaseView
{
	[SerializeField]
	private StatCheckLootMainPageConsoleView m_StatCheckLootMainPageConsoleView;

	[SerializeField]
	private StatCheckLootUnitsPageConsoleView m_StatCheckLootUnitsPageConsoleView;

	protected override void InitializeImpl()
	{
		m_StatCheckLootMainPageConsoleView.Initialize();
		m_StatCheckLootUnitsPageConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StatCheckLootMainPageConsoleView.Bind(base.ViewModel.StatCheckLootMainPageVM);
		m_StatCheckLootUnitsPageConsoleView.Bind(base.ViewModel.StatCheckLootUnitsPageVM);
		AddDisposable(base.ViewModel.CurrentPageType.Subscribe(ShowPage));
	}

	private void ShowPage(StatCheckLootPageType pageType)
	{
		m_StatCheckLootMainPageConsoleView.SetVisibility(pageType == StatCheckLootPageType.Main);
		m_StatCheckLootUnitsPageConsoleView.SetVisibility(pageType == StatCheckLootPageType.Units);
	}
}
