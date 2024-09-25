namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.Console;

public interface IInteractableSystemMapOvertip
{
	bool CanShowTooltip();

	void Interact();

	void ShowTooltip();
}
