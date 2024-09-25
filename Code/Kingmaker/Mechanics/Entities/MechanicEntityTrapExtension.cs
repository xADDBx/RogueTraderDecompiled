using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Mechanics.Entities;

public static class MechanicEntityTrapExtension
{
	public static bool IsTrap(this MechanicEntity entity)
	{
		if (entity is SimpleCaster simpleCaster)
		{
			return simpleCaster.IsTrap;
		}
		return false;
	}
}
