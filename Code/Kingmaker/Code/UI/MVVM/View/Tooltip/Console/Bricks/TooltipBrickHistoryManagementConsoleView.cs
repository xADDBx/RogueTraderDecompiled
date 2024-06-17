using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickHistoryManagementConsoleView : TooltipBrickHistoryManagementView, IConsoleInputHandler
{
	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	protected override void BindViewImplementation()
	{
		m_TitleLabel.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.PreviousButtonInteractable.Subscribe(delegate(bool value)
		{
			m_PreviousButton.Interactable = value;
		}));
		AddDisposable(base.ViewModel.NextButtonInteractable.Subscribe(delegate(bool value)
		{
			m_NextButton.Interactable = value;
		}));
	}

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		AddDisposable(m_PreviousHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnPreviousButtonClick(ownerBehaviour);
		}, 14, base.ViewModel.PreviousButtonInteractable)));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnNextButtonClick(ownerBehaviour);
		}, 15, base.ViewModel.NextButtonInteractable)));
	}

	public void UpdateTooltipBrick()
	{
		base.ViewModel.CheckDirectionButtons();
	}
}
