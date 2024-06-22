using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemContextData
{
	public BaseUnitEntity TargetUnit { get; private set; }

	public StarshipEntity Starship { get; private set; }

	public StarSystemObjectEntity StarSystemObject { get; private set; }

	public void Setup(StarSystemObjectEntity sso, BaseUnitEntity targetUnit = null, StarshipEntity starship = null)
	{
		TargetUnit = targetUnit;
		Starship = starship;
		StarSystemObject = sso;
	}

	public void Reset()
	{
		TargetUnit = null;
		Starship = null;
		StarSystemObject = null;
	}
}
