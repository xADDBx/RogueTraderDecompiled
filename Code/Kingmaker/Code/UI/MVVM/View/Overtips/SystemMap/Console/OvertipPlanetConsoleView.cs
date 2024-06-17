using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;

public class OvertipPlanetConsoleView : OvertipPlanetView, IInteractableSystemMapOvertip, IExplorationUIHandler, ISubscriber, IVendorUIHandler, ISubscriber<IMechanicEntity>, IDialogInteractionHandler
{
	public bool CanShowTooltip()
	{
		return true;
	}

	public void Interact()
	{
		base.ViewModel.RequestVisit();
	}

	public void ShowTooltip()
	{
		this.ShowTooltip(new TooltipTemplateSystemMapPlanet(SystemObject, base.ViewModel.PlanetView.Value));
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_PlanetButton.OnPointerExit();
	}

	public void CloseExplorationScreen()
	{
	}

	public void HandleTradeStarted()
	{
		m_PlanetButton.OnPointerExit();
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		m_PlanetButton.OnPointerExit();
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
	}
}
