using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Alignments;

public static class PartSoulMarkExtensions
{
	[CanBeNull]
	public static PartUnitSoulMark GetOptionalPartSoulMark(this BaseUnitEntity entity)
	{
		return entity.GetOptional<PartUnitSoulMark>();
	}

	[CanBeNull]
	public static PartUnitSoulMark GetOptionalPartSoulMark(this IBaseUnitEntity entity)
	{
		return ((BaseUnitEntity)entity).GetOptionalPartSoulMark();
	}

	public static int GetDelayedSoulMarkValue(this BaseUnitEntity entity, SoulMarkDirection direction)
	{
		return entity.GetOptionalPartSoulMark()?.GetDelayedShiftValue(direction) ?? 0;
	}

	public static void AddDelayedSoulMarkValue(this BaseUnitEntity entity, SoulMarkDirection direction, int value)
	{
		entity.GetOrCreate<PartUnitSoulMark>().AddDelayedShiftValue(direction, value);
	}

	public static void FreeDelayedShift(this BaseUnitEntity entity, SoulMarkDirection direction)
	{
		entity.GetOptionalPartSoulMark()?.FreeDelayedShift(direction);
	}
}
