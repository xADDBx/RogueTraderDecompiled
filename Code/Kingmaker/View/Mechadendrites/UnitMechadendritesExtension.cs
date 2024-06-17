using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.View.Mechadendrites;

public static class UnitMechadendritesExtension
{
	public static bool HasMechadendrites(this MechanicEntity unit)
	{
		return unit?.GetOptional<UnitPartMechadendrites>() != null;
	}
}
