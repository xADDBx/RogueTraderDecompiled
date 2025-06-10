using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;
using Kingmaker.Code.UI.MVVM.View.EtudeCounter.Console;
using Kingmaker.Code.UI.MVVM.View.Formation.Console;
using Kingmaker.Code.UI.MVVM.View.GameOver;
using Kingmaker.Code.UI.MVVM.View.Loot.Console;
using Kingmaker.Code.UI.MVVM.View.Respec;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows;
using Kingmaker.Code.UI.MVVM.View.Subtitle;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;
using Kingmaker.Code.UI.MVVM.View.Transition.Console;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.Vendor;
using Kingmaker.Code.UI.MVVM.View.Vendor.Console;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Console;
using Kingmaker.UI.MVVM.View.GroupChanger.Console;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Surface.Console;

public class SurfaceStaticPartConsoleView : ViewBase<SurfaceStaticPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[SerializeField]
	private ServiceWindowsConsoleView m_ServiceWindowsConsoleView;

	[SerializeField]
	private LootContextConsoleView m_LootContextConsoleView;

	[SerializeField]
	private DialogContextConsoleView m_DialogContextConsoleView;

	[SerializeField]
	private GroupChangerContextConsoleView m_GroupChangerContextConsoleView;

	[SerializeField]
	private UIDestroyViewLink<VendorConsoleView, VendorVM> m_VendorConsoleViewLink;

	[SerializeField]
	private UIDestroyViewLink<VendorSelectingWindowConsoleView, VendorSelectingWindowVM> m_VendorSelectingWindowContextConsoleView;

	[SerializeField]
	private SurfaceHUDConsoleView m_SurfaceHUDConsoleView;

	[SerializeField]
	private UIDestroyViewLink<FormationConsoleView, FormationVM> m_FormationConsoleView;

	[SerializeField]
	private UIDestroyViewLink<CreditsConsoleView, CreditsVM> m_CreditsConsoleView;

	[SerializeField]
	private UIDestroyViewLink<TransitionConsoleView, TransitionVM> m_TransitionConsoleViewLink;

	[SerializeField]
	private EtudeCounterConsoleView m_EtudeCounterView;

	[SerializeField]
	private CharGenContextConsoleView m_CharGenContextConsoleView;

	[SerializeField]
	private RespecContextCommonView m_RespecContextCommonView;

	[SerializeField]
	private UIDestroyViewLink<GameOverConsoleView, GameOverVM> m_GameOverConsoleView;

	[SerializeField]
	private SubtitleView m_SubtitleView;

	[SerializeField]
	private RectTransform m_StaticCanvasRT;

	public void Initialize()
	{
		m_UIVisibilityView.Initialize();
		m_ServiceWindowsConsoleView.Initialize();
		m_GroupChangerContextConsoleView.Initialize();
		m_SurfaceHUDConsoleView.Initialize();
		m_CharGenContextConsoleView.Initialize();
		m_RespecContextCommonView.Initialize();
		m_FormationConsoleView.CustomInitialize = InitFormation;
	}

	private void InitFormation(FormationConsoleView view)
	{
		view.GetComponent<DraggbleWindow>().SetParentRectTransform(m_StaticCanvasRT);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		m_ServiceWindowsConsoleView.Bind(base.ViewModel.ServiceWindowsVM);
		m_LootContextConsoleView.Bind(base.ViewModel.LootContextVM);
		m_DialogContextConsoleView.Bind(base.ViewModel.DialogContextVM);
		m_GroupChangerContextConsoleView.Bind(base.ViewModel.GroupChangerContextVM);
		m_SurfaceHUDConsoleView.Bind(base.ViewModel.SurfaceHUDVM);
		m_SubtitleView.Bind(base.ViewModel.SubtitleVM);
		m_EtudeCounterView.Bind(base.ViewModel.EtudeCounterVM);
		m_CharGenContextConsoleView.Bind(base.ViewModel.CharGenContextVM);
		m_RespecContextCommonView.Bind(base.ViewModel.RespecContextVM);
		AddDisposable(base.ViewModel.FormationVM.Subscribe(m_FormationConsoleView.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsConsoleView.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.TransitionVM.Subscribe(m_TransitionConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.GameOverVM.Subscribe(m_GameOverConsoleView.Bind));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddBaseInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddBaseInput(inputLayer);
	}

	public void AddMainInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddMainInput(inputLayer);
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddCombatInput(inputLayer);
		m_EtudeCounterView.AddInput(inputLayer);
	}

	public void OnShowEscMenu()
	{
		base.ViewModel.HandleShowEscMenu();
	}

	public void HandleBeginCombat()
	{
		m_SurfaceHUDConsoleView.SwitchPartySelector(isEnabled: false);
	}
}
