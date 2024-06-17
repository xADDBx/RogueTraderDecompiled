using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartVisionExtension
{
	[CanBeNull]
	public static PartVision GetVisionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartVision>();
	}
}
