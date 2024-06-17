using System.Collections.Generic;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;

namespace Kingmaker.Globalmap.Exploration;

public interface ICheckForLoot
{
	List<StatDC> GetStats();

	void Check(StatType type, BaseUnitEntity unit);

	SkillCheckResult GetCheckResult();

	ILootable GetLoot();
}
