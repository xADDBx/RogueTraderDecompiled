using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalResourcesBaseView : ViewBase<SystemMapSpaceResourcesVM>
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

	private void DrawEntities()
	{
		m_WidgetListResources.DrawEntries(base.ViewModel.ResourcesVMs, m_SystemMapSpaceResourceViewPrefab);
		m_SystemMapSpaceProfitFactorViewPrefab.Bind(base.ViewModel.JournalOrderProfitFactorVM);
	}
}
