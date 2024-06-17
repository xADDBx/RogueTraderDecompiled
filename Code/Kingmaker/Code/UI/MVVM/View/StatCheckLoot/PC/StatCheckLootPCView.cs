using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.PC;

public class StatCheckLootPCView : StatCheckLootBaseView
{
	[SerializeField]
	private StatCheckLootMainPagePCView m_StatCheckLootMainPagePCView;

	[SerializeField]
	private StatCheckLootUnitsPagePCView m_StatCheckLootUnitsPagePCView;

	protected override void InitializeImpl()
	{
		m_StatCheckLootMainPagePCView.Initialize();
		m_StatCheckLootUnitsPagePCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StatCheckLootMainPagePCView.Bind(base.ViewModel.StatCheckLootMainPageVM);
		m_StatCheckLootUnitsPagePCView.Bind(base.ViewModel.StatCheckLootUnitsPageVM);
		AddDisposable(base.ViewModel.CurrentPageType.Subscribe(ShowPage));
	}

	private void ShowPage(StatCheckLootPageType pageType)
	{
		m_StatCheckLootMainPagePCView.SetVisibility(pageType == StatCheckLootPageType.Main);
		m_StatCheckLootUnitsPagePCView.SetVisibility(pageType == StatCheckLootPageType.Units);
	}
}
