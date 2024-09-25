using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationScanResultsWrapperBaseView : ExplorationComponentWrapperBaseView<ExplorationScanResultsWrapperVM>
{
	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}
}
