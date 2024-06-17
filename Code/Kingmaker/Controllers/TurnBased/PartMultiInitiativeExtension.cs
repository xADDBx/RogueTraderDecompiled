using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Controllers.TurnBased;

public static class PartMultiInitiativeExtension
{
	[CanBeNull]
	public static PartMultiInitiative GetMultiInitiative(this MechanicEntity entity)
	{
		return entity.GetOptional<PartMultiInitiative>();
	}
}
