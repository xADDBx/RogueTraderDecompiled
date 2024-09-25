using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.Console;

public class SpaceDynamicPartConsoleView : ViewBase<SpaceDynamicPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[SerializeField]
	private CanvasGroup m_DynamicCanvasGroup;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityPointerView;

	[SerializeField]
	private SpaceOvertipsConsoleView m_SpaceOvertipsConsoleView;

	[SerializeField]
	private PointMarkersPCView m_SpaceCombatPointMarkersPCView;

	[SerializeField]
	private ConsoleCursor m_ConsoleCursor;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	private InputLayer m_InputLayer;

	public void Initialize()
	{
		m_UIVisibilityDynamicView.Initialize();
		m_UIVisibilityPointerView.Initialize();
		m_SpaceOvertipsConsoleView.Initialize();
		m_SpaceCombatPointMarkersPCView.Initialize(m_DynamicCanvasScalerWorkaround);
		m_ConsoleCursor.Initialize(m_DynamicCanvasScalerWorkaround);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityPointerView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(base.ViewModel.SpaceOvertipsVM.Subscribe(m_SpaceOvertipsConsoleView.Bind));
		AddDisposable(m_ConsoleCursor.Bind());
		AddDisposable(base.ViewModel.SpaceCombatPointMarkersVM.Subscribe(m_SpaceCombatPointMarkersPCView.Bind));
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
