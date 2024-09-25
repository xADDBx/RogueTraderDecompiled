using Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.UI.MVVM.View.Overtips.MapObject;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Console;

public class SpaceOvertipsConsoleView : ViewBase<SpaceOvertipsVM>
{
	[SerializeField]
	private SystemMapOvertipsConsoleView m_SystemMapOvertipsConsoleView;

	[SerializeField]
	private SectorMapOvertipsConsoleView m_SectorMapOvertipsConsoleView;

	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsView;

	[SerializeField]
	private AreaEffectOvertipsCollectionView m_AreaEffectOvertipsCollectionView;

	public void Initialize()
	{
		m_SystemMapOvertipsConsoleView.Initialize();
		m_UnitOvertipsView.Initialize();
		m_AreaEffectOvertipsCollectionView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_SystemMapOvertipsConsoleView.Bind(base.ViewModel.SystemMapOvertipsVM);
		m_SectorMapOvertipsConsoleView.Bind(base.ViewModel.SectorMapOvertipsVM);
		m_UnitOvertipsView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
		m_AreaEffectOvertipsCollectionView.Bind(base.ViewModel.AreaEffectOvertipsCollectionVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
