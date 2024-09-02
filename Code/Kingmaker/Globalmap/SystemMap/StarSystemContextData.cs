using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.Exploration;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemContextData
{
	public BaseUnitEntity TargetUnit { get; private set; }

	public StarshipEntity Starship { get; private set; }

	public StarSystemObjectEntity StarSystemObject { get; private set; }

	public bool IsInteractingWithAnomaly { get; private set; }

	public void Setup(StarSystemObjectEntity sso, BaseUnitEntity targetUnit = null, StarshipEntity starship = null)
	{
		TargetUnit = targetUnit;
		Starship = starship;
		StarSystemObject = sso;
		IsInteractingWithAnomaly = sso is AnomalyEntityData;
	}

	public void Reset()
	{
		TargetUnit = null;
		Starship = null;
		StarSystemObject = null;
		IsInteractingWithAnomaly = false;
	}
}
