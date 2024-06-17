using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Parts;

public static class PartHealthExtension
{
	public class IgnoreWoundThreshold : ContextFlag<IgnoreWoundThreshold>
	{
	}

	[CanBeNull]
	public static PartHealth GetHealthOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartHealth>();
	}

	[CanBeNull]
	public static PartHealth GetHealthOptional(this IMechanicEntity entity)
	{
		return ((MechanicEntity)entity).GetHealthOptional();
	}
}
