using Kingmaker.Code.UI.MVVM.View.InfoWindow.PC;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.PC;

public class TooltipContextPCView : ViewBase<TooltipContextVM>
{
	[SerializeField]
	private TooltipPCView m_TooltipPCView;

	[SerializeField]
	private HintView m_HintPCView;

	[SerializeField]
	private InfoWindowPCView m_InfoWindowPCView;

	[SerializeField]
	private InfoWindowPCView m_GlossaryInfoWindowPCView;

	[SerializeField]
	private ComparativeTooltipPCView m_ComparativeTooltipPCView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_TooltipPCView.Initialize();
			m_HintPCView.Initialize();
			m_InfoWindowPCView.Initialize();
			m_GlossaryInfoWindowPCView.Initialize();
			m_ComparativeTooltipPCView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.TooltipVM.Subscribe(m_TooltipPCView.Bind));
		AddDisposable(base.ViewModel.HintVM.Subscribe(m_HintPCView.Bind));
		AddDisposable(base.ViewModel.InfoWindowVM.Subscribe(m_InfoWindowPCView.Bind));
		AddDisposable(base.ViewModel.GlossaryInfoWindowVM.Subscribe(m_GlossaryInfoWindowPCView.Bind));
		AddDisposable(base.ViewModel.ComparativeTooltipVM.Subscribe(m_ComparativeTooltipPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
