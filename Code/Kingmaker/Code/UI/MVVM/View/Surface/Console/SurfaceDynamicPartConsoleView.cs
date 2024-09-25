using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.VariativeInteraction.Console;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Surface.Console;

public class SurfaceDynamicPartConsoleView : ViewBase<SurfaceDynamicPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[SerializeField]
	private CanvasGroup m_DynamicCanvasGroup;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityPointerView;

	[FormerlySerializedAs("VariativeInteractionView")]
	[SerializeField]
	private VariativeInteractionConsoleView m_VariativeInteractionView;

	[SerializeField]
	private SurfaceOvertipsConsoleView m_SurfaceOvertipsView;

	[SerializeField]
	private PointMarkersPCView m_PointMarkersPCView;

	[SerializeField]
	private ConsoleCursor m_ConsoleCursor;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	public void Initialize()
	{
		m_UIVisibilityDynamicView.Initialize();
		m_UIVisibilityPointerView.Initialize();
		m_VariativeInteractionView.Initialize();
		m_SurfaceOvertipsView.Initialize();
		m_PointMarkersPCView.Initialize(m_DynamicCanvasScalerWorkaround);
		m_ConsoleCursor.Initialize(m_DynamicCanvasScalerWorkaround);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityPointerView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(base.ViewModel.SurfaceOvertipsVM.Subscribe(m_SurfaceOvertipsView.Bind));
		AddDisposable(base.ViewModel.VariativeInteractionVM.Subscribe(m_VariativeInteractionView.Bind));
		AddDisposable(base.ViewModel.PointMarkersVM.Subscribe(m_PointMarkersPCView.Bind));
		AddDisposable(m_ConsoleCursor.Bind());
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OnUpdate()
	{
		if (m_DynamicCanvasGroup.blocksRaycasts != ConsoleCursor.Instance.IsActive)
		{
			m_DynamicCanvasGroup.blocksRaycasts = ConsoleCursor.Instance.IsActive;
		}
	}
}
