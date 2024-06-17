using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPlanetInfoVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly BlueprintPlanet BlueprintPlanet;

	public readonly BlueprintStarSystemMap BlueprintStarSystemMap;

	public readonly bool IsExplored;

	public readonly bool IsColonized;

	public TooltipBrickPlanetInfoVM(BlueprintPlanet blueprintPlanet, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		TooltipBrickPlanetInfoVM tooltipBrickPlanetInfoVM = this;
		Name = (string.IsNullOrWhiteSpace(blueprintPlanet.Name) ? "Empty Name" : blueprintPlanet.Name);
		Icon = UIConfig.Instance.UIIcons.GetPlanetIcon(blueprintPlanet.Type);
		BlueprintPlanet = blueprintPlanet;
		BlueprintStarSystemMap = blueprintStarSystemMap;
		IsExplored = Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.Planet == tooltipBrickPlanetInfoVM.BlueprintPlanet).EmptyIfNull().Any();
		IsColonized = Game.Instance.Player.ColoniesState.Colonies.Any((ColoniesState.ColonyData c) => c.Planet == blueprintPlanet);
	}
}
