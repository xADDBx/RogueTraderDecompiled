namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationPointOfInterestListWrapperVM : ExplorationUIComponentWrapperVM
{
	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Exploration | ExplorationUISection.Colony;
}
