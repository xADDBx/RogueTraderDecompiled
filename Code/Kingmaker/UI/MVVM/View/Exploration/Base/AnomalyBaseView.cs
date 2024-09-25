using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class AnomalyBaseView<TStatCheckLootView> : ViewBase<AnomalyVM> where TStatCheckLootView : StatCheckLootBaseView
{
	[SerializeField]
	private TStatCheckLootView m_StatCheckLootView;

	public void Initialize()
	{
		m_StatCheckLootView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_StatCheckLootView.Bind(base.ViewModel.StatCheckLootAnomalyVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
