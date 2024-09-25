using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;

public class CharInfoSkillsAndWeaponsVM : CharInfoComponentWithLevelUpVM
{
	public readonly CharInfoAbilityScoresBlockVM AbilityScoresBlockVM;

	public readonly CharInfoSkillsBlockVM SkillsBlockVM;

	public readonly CharInfoWeaponsBlockVM WeaponsBlockVM;

	public CharInfoSkillsAndWeaponsVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		AddDisposable(AbilityScoresBlockVM = new CharInfoAbilityScoresBlockVM(unit));
		AddDisposable(SkillsBlockVM = new CharInfoSkillsBlockVM(unit, levelUpManager));
		AddDisposable(WeaponsBlockVM = new CharInfoWeaponsBlockVM(unit));
	}
}
