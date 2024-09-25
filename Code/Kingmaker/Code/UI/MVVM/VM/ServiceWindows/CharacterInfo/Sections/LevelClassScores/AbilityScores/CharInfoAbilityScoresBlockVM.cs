using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.UnitDescription;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoAbilityScoresBlockVM : CharInfoBaseAbilityScoresBlockVM
{
	public static List<StatType> AbilitiesOrdered = new List<StatType>
	{
		StatType.WarhammerWeaponSkill,
		StatType.WarhammerBallisticSkill,
		StatType.WarhammerStrength,
		StatType.WarhammerToughness,
		StatType.WarhammerAgility,
		StatType.WarhammerIntelligence,
		StatType.WarhammerPerception,
		StatType.WarhammerWillpower,
		StatType.WarhammerFellowship
	};

	protected override List<StatType> StatsTypes { get; } = AbilitiesOrdered;


	public CharInfoAbilityScoresBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
	}

	public CharInfoAbilityScoresBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit, null, null)
	{
	}

	public CharInfoAbilityScoresBlockVM(UnitDescription.StatsData statsData)
	{
	}
}
