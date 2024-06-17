using Kingmaker.Code.UI.MVVM.View.Overtips.PC;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.VariativeInteraction;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Surface.PC;

public class SurfaceDynamicPartPCView : ViewBase<SurfaceDynamicPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityPointerView;

	[Space]
	[SerializeField]
	private SurfaceOvertipsPCView m_SurfaceOvertipsView;

	[FormerlySerializedAs("VariativeInteractionView")]
	[SerializeField]
	private VariativeInteractionPCView m_VariativeInteractionView;

	[SerializeField]
	private PointMarkersPCView m_PointMarkersPCView;

	[SerializeField]
	private PCCursor m_Cursor;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	public void Initialize()
	{
		m_UIVisibilityDynamicView.Initialize();
		m_UIVisibilityPointerView.Initialize();
		m_SurfaceOvertipsView.Initialize();
		m_VariativeInteractionView.Initialize();
		m_PointMarkersPCView.Initialize(m_DynamicCanvasScalerWorkaround);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityPointerView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(m_Cursor.Bind());
		AddDisposable(base.ViewModel.SurfaceOvertipsVM.Subscribe(m_SurfaceOvertipsView.Bind));
		AddDisposable(base.ViewModel.VariativeInteractionVM.Subscribe(m_VariativeInteractionView.Bind));
		AddDisposable(base.ViewModel.PointMarkersVM.Subscribe(m_PointMarkersPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
