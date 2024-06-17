namespace Kingmaker.EntitySystem.Stats;

public interface IModifiableValueDependent
{
	ModifiableValue BaseStats { get; }

	int BaseStatBonus { get; }
}
