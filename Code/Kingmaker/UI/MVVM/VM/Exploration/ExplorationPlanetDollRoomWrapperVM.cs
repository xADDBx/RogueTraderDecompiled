namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationPlanetDollRoomWrapperVM : ExplorationUIComponentWrapperVM
{
	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.NotScanned | ExplorationUISection.Exploration | ExplorationUISection.Colony;
}
