using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickOverseerConsolePaperView : TooltipBrickOverseerPaperView, IConsoleTooltipBrick, IConsoleInputHandler
{
	private BoolReactiveProperty m_hintVisible = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_MultiButton.OnFocus.AddListener(UpdateHintsHandler);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_MultiButton.OnFocus.RemoveListener(UpdateHintsHandler);
	}

	private void UpdateHintsHandler(bool value)
	{
		m_hintVisible.Value = m_MultiButton.IsFocus;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_MultiButton);
	}

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			RequestShowInspectConsole();
		}, 8, m_hintVisible);
		AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharacterSheet.ShowTooltip));
		AddDisposable(inputBindStruct);
	}

	private void RequestShowInspectConsole()
	{
		if (m_MultiButton.IsFocus)
		{
			m_MultiButton.SetFocus(value: false);
			EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
			{
				h.HandleUnitConsoleInvoke(base.ViewModel.UnitToShow);
			});
		}
	}

	public void UpdateTooltipBrick()
	{
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
