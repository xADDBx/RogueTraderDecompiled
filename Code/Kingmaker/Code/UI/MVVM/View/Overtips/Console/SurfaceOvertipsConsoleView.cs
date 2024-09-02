using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.UI.MVVM.View.Overtips.MapObject;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Console;

public class SurfaceOvertipsConsoleView : ViewBase<SurfaceOvertipsVM>
{
	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsView;

	[SerializeField]
	private LightweightUnitOvertipsCollectionView m_LightweightUnitOvertipsView;

	[SerializeField]
	private MapObjectOvertipsConsoleView m_MapObjectOvertipsConsoleView;

	[SerializeField]
	private LocatorOvertipsCollectionView m_LocatorOvertipsCollectionView;

	public void Initialize()
	{
		m_UnitOvertipsView.Initialize();
		m_LightweightUnitOvertipsView.Initialize();
		m_MapObjectOvertipsConsoleView.Initialize();
		m_LocatorOvertipsCollectionView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UnitOvertipsView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
		m_LightweightUnitOvertipsView.Bind(base.ViewModel.LightweightUnitOvertipsCollectionVM);
		m_MapObjectOvertipsConsoleView.Bind(base.ViewModel.MapObjectOvertipsVM);
		m_LocatorOvertipsCollectionView.Bind(base.ViewModel.LocatorOvertipsCollectionVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
