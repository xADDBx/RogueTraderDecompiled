using System;

namespace Kingmaker.UI.MVVM.VM.Exploration;

[Flags]
public enum ExplorationUISection
{
	None = 0,
	NotScanned = 1,
	Exploration = 2,
	Colony = 4,
	ColonyProjects = 8
}
