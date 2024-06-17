using Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
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

	public void Initialize()
	{
		m_SystemMapOvertipsConsoleView.Initialize();
		m_UnitOvertipsView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_SystemMapOvertipsConsoleView.Bind(base.ViewModel.SystemMapOvertipsVM);
		m_SectorMapOvertipsConsoleView.Bind(base.ViewModel.SectorMapOvertipsVM);
		m_UnitOvertipsView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
