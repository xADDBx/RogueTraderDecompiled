using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.PC;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.UI.MVVM.View.Overtips.MapObject;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.PC;

public class SurfaceOvertipsPCView : ViewBase<SurfaceOvertipsVM>
{
	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsPCView;

	[SerializeField]
	private LightweightUnitOvertipsCollectionView m_LightweightUnitOvertipsPCView;

	[SerializeField]
	private MapObjectOvertipsPCView m_MapObjectOvertipsPCView;

	[SerializeField]
	private LocatorOvertipsCollectionView m_LocatorOvertipsCollectionView;

	public void Initialize()
	{
		m_UnitOvertipsPCView.Initialize();
		m_LightweightUnitOvertipsPCView.Initialize();
		m_MapObjectOvertipsPCView.Initialize();
		m_LocatorOvertipsCollectionView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UnitOvertipsPCView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
		m_LightweightUnitOvertipsPCView.Bind(base.ViewModel.LightweightUnitOvertipsCollectionVM);
		m_MapObjectOvertipsPCView.Bind(base.ViewModel.MapObjectOvertipsVM);
		m_LocatorOvertipsCollectionView.Bind(base.ViewModel.LocatorOvertipsCollectionVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
