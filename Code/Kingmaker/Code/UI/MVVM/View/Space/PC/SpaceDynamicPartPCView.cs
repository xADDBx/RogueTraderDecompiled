using Kingmaker.Code.UI.MVVM.View.Overtips.PC;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class SpaceDynamicPartPCView : ViewBase<SpaceDynamicPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityPointerView;

	[Space]
	[SerializeField]
	private SpaceOvertipsPCView m_SpaceOvertipsView;

	[SerializeField]
	private PointMarkersPCView m_SpaceCombatPointMarkersPCView;

	[SerializeField]
	private PCCursor m_Cursor;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	public void Initialize()
	{
		m_UIVisibilityDynamicView.Initialize();
		m_UIVisibilityPointerView.Initialize();
		m_SpaceOvertipsView.Initialize();
		m_SpaceCombatPointMarkersPCView.Initialize(m_DynamicCanvasScalerWorkaround);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityPointerView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(base.ViewModel.SpaceOvertipsVM.Subscribe(m_SpaceOvertipsView.Bind));
		AddDisposable(m_Cursor.Bind());
		AddDisposable(base.ViewModel.SpaceCombatPointMarkersVM.Subscribe(m_SpaceCombatPointMarkersPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
