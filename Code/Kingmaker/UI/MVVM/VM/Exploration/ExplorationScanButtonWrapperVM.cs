using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.GameCommands;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationScanButtonWrapperVM : ExplorationUIComponentWrapperVM
{
	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.NotScanned;

	public void Interact()
	{
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			UISounds.Instance.Sounds.SpaceExploration.PlanetScanAnimation.Play();
			Game.Instance.GameCommandQueue.ScanStarSystemObject(StarSystemObjectStateVM.StarSystemObjectView.Value.Data);
		}
	}
}
