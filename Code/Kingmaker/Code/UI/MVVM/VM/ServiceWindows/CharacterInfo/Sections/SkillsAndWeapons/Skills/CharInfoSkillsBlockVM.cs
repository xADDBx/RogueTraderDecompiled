using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;

public class CharInfoSkillsBlockVM : CharInfoBaseAbilityScoresBlockVM
{
	public static List<StatType> SkillsOrdered = new List<StatType>
	{
		StatType.SkillAthletics,
		StatType.SkillAwareness,
		StatType.SkillCarouse,
		StatType.SkillPersuasion,
		StatType.SkillDemolition,
		StatType.SkillMedicae,
		StatType.SkillLogic,
		StatType.SkillLoreXenos,
		StatType.SkillLoreWarp,
		StatType.SkillLoreImperium,
		StatType.SkillTechUse,
		StatType.SkillCommerce,
		StatType.SkillCoercion
	};

	protected override List<StatType> StatsTypes { get; } = SkillsOrdered;


	public CharInfoSkillsBlockVM(StatsContainer stats)
		: base(stats)
	{
	}

	public CharInfoSkillsBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
	}
}
