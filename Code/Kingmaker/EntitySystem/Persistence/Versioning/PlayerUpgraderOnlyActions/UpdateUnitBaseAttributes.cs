using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[TypeId("40099302b74e604419bf2107c9a4103a")]
public class UpdateUnitBaseAttributes : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	public override string GetCaption()
	{
		return $"Update base attributes of {m_Unit}";
	}

	protected override void RunActionOverride()
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		PartStatsContainer stats = value.Stats;
		stats.GetStat(StatType.WarhammerBallisticSkill, canBeNull: true).BaseValue = value.Blueprint.WarhammerBallisticSkill;
		stats.GetStat(StatType.WarhammerWeaponSkill, canBeNull: true).BaseValue = value.Blueprint.WarhammerWeaponSkill;
		stats.GetStat(StatType.WarhammerStrength, canBeNull: true).BaseValue = value.Blueprint.WarhammerStrength;
		stats.GetStat(StatType.WarhammerToughness, canBeNull: true).BaseValue = value.Blueprint.WarhammerToughness;
		stats.GetStat(StatType.WarhammerAgility, canBeNull: true).BaseValue = value.Blueprint.WarhammerAgility;
		stats.GetStat(StatType.WarhammerIntelligence, canBeNull: true).BaseValue = value.Blueprint.WarhammerIntelligence;
		stats.GetStat(StatType.WarhammerWillpower, canBeNull: true).BaseValue = value.Blueprint.WarhammerWillpower;
		stats.GetStat(StatType.WarhammerPerception, canBeNull: true).BaseValue = value.Blueprint.WarhammerPerception;
		stats.GetStat(StatType.WarhammerFellowship, canBeNull: true).BaseValue = value.Blueprint.WarhammerFellowship;
	}
}
