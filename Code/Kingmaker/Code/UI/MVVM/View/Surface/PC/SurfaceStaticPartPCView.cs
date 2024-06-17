using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.Dialog;
using Kingmaker.Code.UI.MVVM.View.EtudeCounter;
using Kingmaker.Code.UI.MVVM.View.Formation;
using Kingmaker.Code.UI.MVVM.View.GameOver;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.View.Respec;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows;
using Kingmaker.Code.UI.MVVM.View.Subtitle;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Code.UI.MVVM.View.Transition.PC;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.Vendor;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.EtudeCounter;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.PC;
using Kingmaker.UI.MVVM.View.GroupChanger.PC;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Surface.PC;

public class SurfaceStaticPartPCView : ViewBase<SurfaceStaticPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[SerializeField]
	private ServiceWindowsPCView m_ServiceWindowsPCView;

	[SerializeField]
	private LootContextPCView m_LootContextPCView;

	[SerializeField]
	private DialogContextPCView m_DialogContextPCView;

	[SerializeField]
	private GroupChangerContextPCView m_GroupChangerContextPCView;

	[SerializeField]
	private UIViewLink<TransitionPCView, TransitionVM> m_TransitionPCViewLink;

	[SerializeField]
	private UIViewLink<FormationPCView, FormationVM> m_FormationPCView;

	[SerializeField]
	private UIViewLink<CreditsPCView, CreditsVM> m_CreditsPCView;

	[SerializeField]
	private UIViewLink<VendorPCView, VendorVM> m_VendorPCViewLink;

	[SerializeField]
	private UIViewLink<VendorSelectingWindowPCView, VendorSelectingWindowVM> m_VendorSelectingWindowContextPCView;

	[SerializeField]
	private SurfaceHUDPCView SurfaceHUDView;

	[SerializeField]
	private GameOverPCView m_GameOverPCView;

	[SerializeField]
	private SubtitleView m_SubtitleView;

	[SerializeField]
	private UIViewLink<EtudeCounterView, EtudeCounterVM> m_EtudeCounterView;

	[SerializeField]
	private CharGenContextPCView m_CharGenContextPCView;

	[SerializeField]
	private RespecContextCommonView m_RespecContextCommonView;

	[SerializeField]
	private RectTransform m_StaticCanvasRT;

	public void Initialize()
	{
		m_UIVisibilityView.Initialize();
		m_ServiceWindowsPCView.Initialize();
		m_LootContextPCView.Initialize();
		m_GroupChangerContextPCView.Initialize();
		SurfaceHUDView.Initialize();
		m_GameOverPCView.Initialize();
		m_CharGenContextPCView.Initialize();
		m_RespecContextCommonView.Initialize();
		m_FormationPCView.CustomInitialize = InitFormation;
	}

	private void InitFormation(FormationPCView view)
	{
		view.GetComponent<DraggbleWindow>().SetParentRectTransform(m_StaticCanvasRT);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		m_ServiceWindowsPCView.Bind(base.ViewModel.ServiceWindowsVM);
		m_LootContextPCView.Bind(base.ViewModel.LootContextVM);
		m_DialogContextPCView.Bind(base.ViewModel.DialogContextVM);
		m_GroupChangerContextPCView.Bind(base.ViewModel.GroupChangerContextVM);
		SurfaceHUDView.Bind(base.ViewModel.SurfaceHUDVM);
		m_SubtitleView.Bind(base.ViewModel.SubtitleVM);
		m_EtudeCounterView.Bind(base.ViewModel.EtudeCounterVM);
		m_CharGenContextPCView.Bind(base.ViewModel.CharGenContextVM);
		m_RespecContextCommonView.Bind(base.ViewModel.RespecContextVM);
		AddDisposable(base.ViewModel.TransitionVM.Subscribe(m_TransitionPCViewLink.Bind));
		AddDisposable(base.ViewModel.FormationVM.Subscribe(m_FormationPCView.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsPCView.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorPCViewLink.Bind));
		AddDisposable(base.ViewModel.GameOverVM.Subscribe(m_GameOverPCView.Bind));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
