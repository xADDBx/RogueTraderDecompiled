namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationVisualElementsWrapperVM : ExplorationUIComponentWrapperVM
{
	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.NotScanned | ExplorationUISection.Exploration;
}
