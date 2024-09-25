using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.SystemMap;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationSpaceResourcesBaseView : ViewBase<ExplorationSpaceResourcesVM>
{
	[Header("Resources")]
	[SerializeField]
	private WidgetListMVVM m_WidgetListResources;

	[SerializeField]
	private SystemMapSpaceResourceView m_SystemMapSpaceResourceViewPrefab;

	[SerializeField]
	private SystemMapSpaceProfitFactorView m_SystemMapSpaceProfitFactorViewPrefab;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.RefreshData.Subscribe(delegate
		{
			DrawEntities();
		}));
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		list.AddRange(m_WidgetListResources.GetNavigationEntities());
		list.Add(m_SystemMapSpaceProfitFactorViewPrefab);
		return list;
	}

	private void DrawEntities()
	{
		m_WidgetListResources.DrawEntries(base.ViewModel.ResourcesVMs, m_SystemMapSpaceResourceViewPrefab);
		m_SystemMapSpaceProfitFactorViewPrefab.Bind(base.ViewModel.JournalOrderProfitFactorVM);
	}
}
