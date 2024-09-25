using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("7bdb8039e4f8442fbc541a34e590d8fa")]
public class BlueprintEncyclopediaPlanetTypePage : BlueprintEncyclopediaPage
{
	public class PlanetBlock : IBlock
	{
		public BlueprintPlanet Planet;

		public BlueprintStarSystemMap StarSystem;

		public Colony Colony;

		public bool IsReportedToAdministratum;

		public BlueprintEncyclopediaPlanetTypePage Entry;

		public PlanetBlock(BlueprintEncyclopediaPlanetTypePage entry)
		{
			Entry = entry;
		}
	}

	[SerializeField]
	public BlueprintPlanet.PlanetType PlanetType;

	public bool IsAvailable => Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.Planet?.Type == PlanetType).EmptyIfNull().Any();

	public override List<IBlock> GetBlocks()
	{
		IEnumerable<PlanetExplorationInfo> enumerable = Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.Planet.Type == PlanetType).EmptyIfNull();
		List<IBlock> blocks = base.GetBlocks();
		foreach (PlanetExplorationInfo planetInfo in enumerable)
		{
			PlanetBlock item = new PlanetBlock(this)
			{
				Planet = planetInfo.Planet,
				StarSystem = planetInfo.StarSystemMap,
				IsReportedToAdministratum = planetInfo.IsReportedToAdministratum,
				Colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyInfo) => colonyInfo.Planet == planetInfo.Planet)?.Colony
			};
			blocks.Add(item);
		}
		return blocks;
	}
}
