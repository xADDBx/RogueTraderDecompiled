using Kingmaker.Pathfinding;
using Kingmaker.View.Covers;

namespace Kingmaker.Controllers;

public interface IDynamicCoverProvider
{
	NodeList Nodes { get; }

	LosCalculations.CoverType CoverType { get; }
}
