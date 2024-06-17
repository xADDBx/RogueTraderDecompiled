namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public interface IExplorationComponentEntity
{
	bool CanInteract();

	bool CanShowTooltip();

	void Interact();

	void ShowTooltip();
}
