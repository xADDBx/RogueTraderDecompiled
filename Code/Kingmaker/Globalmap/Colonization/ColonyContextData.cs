using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;

namespace Kingmaker.Globalmap.Colonization;

public class ColonyContextData : ContextData<ColonyContextData>
{
	public Colony Colony;

	public BlueprintPlanet Planet;

	public BlueprintStarSystemMap StarSystemArea;

	[CanBeNull]
	public BlueprintColonyEvent Event;

	public ColonyContextData Setup([NotNull] Colony colony, [CanBeNull] BlueprintColonyEvent colonyEvent)
	{
		Colony = colony;
		Planet = colony.Planet;
		StarSystemArea = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony == colony)?.Area;
		Event = colonyEvent;
		return this;
	}

	protected override void Reset()
	{
		Colony = null;
		Planet = null;
		StarSystemArea = null;
		Event = null;
	}
}
