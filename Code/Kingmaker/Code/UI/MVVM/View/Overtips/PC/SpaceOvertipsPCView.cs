using Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.PC;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.PC;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.PC;

public class SpaceOvertipsPCView : ViewBase<SpaceOvertipsVM>
{
	[SerializeField]
	private SystemMapOvertipsPCView m_SystemMapOvertipsPCView;

	[SerializeField]
	private SectorMapOvertipsPCView m_SectorMapOvertipsPCView;

	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsPCView;

	public void Initialize()
	{
		m_SystemMapOvertipsPCView.Initialize();
		m_UnitOvertipsPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_SystemMapOvertipsPCView.Bind(base.ViewModel.SystemMapOvertipsVM);
		m_SectorMapOvertipsPCView.Bind(base.ViewModel.SectorMapOvertipsVM);
		m_UnitOvertipsPCView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
