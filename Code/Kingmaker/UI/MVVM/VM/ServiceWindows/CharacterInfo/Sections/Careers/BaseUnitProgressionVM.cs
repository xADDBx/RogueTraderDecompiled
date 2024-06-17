using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UnitLogic.Levelup;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;

public abstract class BaseUnitProgressionVM : CharInfoComponentVM
{
	public readonly ReactiveProperty<CareerPathVM> CurrentCareer = new ReactiveProperty<CareerPathVM>();

	public readonly ReactiveProperty<IRankEntrySelectItem> CurrentRankEntryItem = new ReactiveProperty<IRankEntrySelectItem>();

	public readonly ReactiveProperty<IRankEntrySelectItem> FirstAvailableEntryItem = new ReactiveProperty<IRankEntrySelectItem>();

	public readonly ReactiveCommand<IRankEntrySelectItem> OnRepeatedCurrentRankEntryItem = new ReactiveCommand<IRankEntrySelectItem>();

	protected readonly IReactiveProperty<LevelUpManager> m_LevelUpManager;

	public abstract CharInfoExperienceVM CharInfoExperienceVM { get; }

	public abstract UnitBackgroundBlockVM UnitBackgroundBlockVM { get; }

	public LevelUpManager LevelUpManager => m_LevelUpManager?.Value;

	protected BaseUnitProgressionVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit)
	{
		m_LevelUpManager = levelUpManager;
	}

	public abstract void SetCareerPath(CareerPathVM careerPathVM, bool force = false);

	public abstract void SelectCareerPath();

	public abstract void SetRankEntry(IRankEntrySelectItem rankEntryItem);

	public abstract void Commit();

	public abstract void SetPreviousState(bool saveSelections = false);
}
