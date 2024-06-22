using JetBrains.Annotations;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class ColonyContextData : IHashable
{
	public Colony Colony;

	public BlueprintPlanet Planet;

	public BlueprintStarSystemMap StarSystemArea;

	[CanBeNull]
	public BlueprintColonyEvent Event;

	public void Setup([NotNull] Colony colony, [CanBeNull] BlueprintColonyEvent colonyEvent)
	{
		Colony = colony;
		Planet = colony.Planet;
		StarSystemArea = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony == colony)?.Area;
		Event = colonyEvent;
	}

	public void Reset()
	{
		Colony = null;
		Planet = null;
		StarSystemArea = null;
		Event = null;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
