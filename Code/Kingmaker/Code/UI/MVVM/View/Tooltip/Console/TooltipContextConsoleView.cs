using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.PC;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console;

public class TooltipContextConsoleView : ViewBase<TooltipContextVM>
{
	[SerializeField]
	private TooltipConsoleView m_TooltipConsoleView;

	[SerializeField]
	private HintView m_HintView;

	[SerializeField]
	private ComparativeTooltipConsoleView m_ComparativeTooltipConsoleView;

	[SerializeField]
	private InfoWindowConsoleView m_InfoWindowConsoleView;

	[SerializeField]
	private InfoWindowConsoleView m_GlossaryInfoWindowConsoleView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_TooltipConsoleView.Initialize();
			m_HintView.Initialize();
			m_ComparativeTooltipConsoleView.Initialize();
			m_InfoWindowConsoleView.Initialize();
			m_GlossaryInfoWindowConsoleView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.TooltipVM.Subscribe(m_TooltipConsoleView.Bind));
		AddDisposable(base.ViewModel.HintVM.Subscribe(m_HintView.Bind));
		AddDisposable(base.ViewModel.ComparativeTooltipVM.Subscribe(m_ComparativeTooltipConsoleView.Bind));
		AddDisposable(base.ViewModel.InfoWindowVM.Subscribe(m_InfoWindowConsoleView.Bind));
		AddDisposable(base.ViewModel.GlossaryInfoWindowVM.Subscribe(m_GlossaryInfoWindowConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
