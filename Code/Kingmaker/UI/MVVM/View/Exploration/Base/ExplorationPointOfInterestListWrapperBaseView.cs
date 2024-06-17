using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationPointOfInterestListWrapperBaseView : ExplorationComponentWrapperBaseView<ExplorationPointOfInterestListWrapperVM>
{
	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}
}
