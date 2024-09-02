using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemMapSpaceResourcesPCView : ViewBase<SystemMapSpaceResourcesVM>
{
	[Header("Resources")]
	[SerializeField]
	private WidgetListMVVM m_WidgetListResources;

	[SerializeField]
	private SystemMapSpaceResourceView m_SystemMapSpaceResourceViewPrefab;

	[SerializeField]
	private SystemMapSpaceProfitFactorView m_SystemMapSpaceProfitFactorViewPrefab;

	public static SystemMapSpaceResourcesPCView Instance;

	public WidgetListMVVM WidgetListResources => m_WidgetListResources;

	public SystemMapSpaceProfitFactorView SystemMapSpaceProfitFactorViewPrefab => m_SystemMapSpaceProfitFactorViewPrefab;

	protected override void BindViewImplementation()
	{
		Instance = this;
		AddDisposable(base.ViewModel.ShouldShow.CombineLatest(base.ViewModel.IsInSpaceCombat, (bool shouldShow, bool isInSpaceCombat) => new { shouldShow, isInSpaceCombat }).Subscribe(value =>
		{
			base.gameObject.SetActive(value.shouldShow && !value.isInSpaceCombat);
		}));
		AddDisposable(base.ViewModel.RefreshData.Subscribe(delegate
		{
			DrawEntities();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Instance = null;
	}

	private void DrawEntities()
	{
		if (base.ViewModel != null)
		{
			if (base.ViewModel.ResourcesVMs != null && m_SystemMapSpaceResourceViewPrefab != null)
			{
				m_WidgetListResources.DrawEntries(base.ViewModel.ResourcesVMs, m_SystemMapSpaceResourceViewPrefab);
			}
			m_SystemMapSpaceProfitFactorViewPrefab.Bind(base.ViewModel.JournalOrderProfitFactorVM);
		}
	}
}
