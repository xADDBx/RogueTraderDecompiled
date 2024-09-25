using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartSummonedMonsterExtension
{
	[CanBeNull]
	public static UnitPartSummonedMonster GetSummonedMonsterOption(this MechanicEntity entity)
	{
		return entity.GetOptional<UnitPartSummonedMonster>();
	}
}
