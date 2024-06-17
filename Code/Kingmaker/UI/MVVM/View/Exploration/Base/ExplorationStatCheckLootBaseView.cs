using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationStatCheckLootBaseView<TStatCheckLootView> : ExplorationComponentBaseView<ExplorationStatCheckLootVM> where TStatCheckLootView : StatCheckLootBaseView
{
	[SerializeField]
	private TStatCheckLootView m_StatCheckLootView;

	public void Initialize()
	{
		m_StatCheckLootView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StatCheckLootView.Bind(base.ViewModel.StatCheckLootPointOfInterestVM);
	}
}
