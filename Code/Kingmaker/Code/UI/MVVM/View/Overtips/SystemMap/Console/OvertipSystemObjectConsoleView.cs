using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;

public class OvertipSystemObjectConsoleView : OvertipSystemObjectView, IInteractableSystemMapOvertip, IExplorationUIHandler, ISubscriber, IVendorUIHandler, ISubscriber<IMechanicEntity>, IDialogInteractionHandler
{
	public bool CanShowTooltip()
	{
		return false;
	}

	public void Interact()
	{
		base.ViewModel.RequestVisit();
	}

	public void ShowTooltip()
	{
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_SystemObjectButton.OnPointerExit();
	}

	public void CloseExplorationScreen()
	{
	}

	public void HandleTradeStarted()
	{
		m_SystemObjectButton.OnPointerExit();
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		m_SystemObjectButton.OnPointerExit();
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
	}
}
