using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores;

public class CharInfoLevelClassScoresVM : CharInfoComponentWithLevelUpVM
{
	public InventoryDollAdditionalStatsVM AdditionalStatsVM;

	public CharInfoExperienceVM Experience { get; }

	public CharInfoClassesListVM Classes { get; }

	public CharInfoAbilityScoresBlockVM AbilityScores { get; }

	public CharInfoLevelClassScoresVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		AddDisposable(Experience = new CharInfoExperienceVM(unit));
		AddDisposable(Classes = new CharInfoClassesListVM(unit, isMythic: false));
		AddDisposable(AbilityScores = new CharInfoAbilityScoresBlockVM(unit, levelUpManager));
		AddDisposable(AdditionalStatsVM = new InventoryDollAdditionalStatsVM(unit, levelUpManager));
	}
}
