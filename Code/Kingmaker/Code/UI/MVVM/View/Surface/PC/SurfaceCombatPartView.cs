using Kingmaker.Code.UI.MVVM.View.Combat;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Surface.PC;

public class SurfaceCombatPartView : ViewBase<SurfaceCombatPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[SerializeField]
	private LineOfSightControllerView m_SightControllerPCView;

	public void Initialize()
	{
		m_UIVisibilityView.Initialize();
		m_SightControllerPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		m_SightControllerPCView.Bind(base.ViewModel.LineOfSightControllerVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
