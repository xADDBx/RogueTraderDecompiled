using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoBaseAbilityScoresBlockVM : CharInfoComponentWithLevelUpVM
{
	public readonly AutoDisposingList<CharInfoStatVM> Stats = new AutoDisposingList<CharInfoStatVM>();

	public readonly ReactiveCommand OnStatsUpdated = new ReactiveCommand();

	protected virtual List<StatType> StatsTypes { get; }

	protected CharInfoBaseAbilityScoresBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager, StatsContainer stats)
		: base(unit, levelUpManager)
	{
		AddDisposable(PreviewUnit.Skip(1).Subscribe(delegate
		{
			HandleUpdatePreviewUnit();
		}));
	}

	protected CharInfoBaseAbilityScoresBlockVM()
		: base(null, null)
	{
	}

	protected CharInfoBaseAbilityScoresBlockVM(StatsContainer stats)
		: this(null, null, stats)
	{
	}

	protected CharInfoBaseAbilityScoresBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: this(unit, levelUpManager, unit.Value.Stats.Container)
	{
	}

	private void HandleUpdatePreviewUnit()
	{
		RefreshData();
	}

	public override void HandleUICommitChanges()
	{
		RefreshData();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		ClearStats();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		FillStats(Unit.Value.Stats.Container, PreviewUnit.Value?.Stats.Container);
	}

	protected void FillStats(StatsContainer stats, StatsContainer previewStats)
	{
		ClearStats();
		foreach (StatType statsType in StatsTypes)
		{
			ModifiableValue stat = stats.GetStat(statsType);
			ModifiableValue previewStat = previewStats?.GetStat(statsType);
			CharInfoStatVM charInfoStatVM = new CharInfoStatVM(stat, previewStat);
			AddDisposable(charInfoStatVM);
			Stats.Add(charInfoStatVM);
		}
		UpdateRecommendedMarks();
		OnStatsUpdated.Execute();
	}

	private void ClearStats()
	{
		Stats.Clear();
	}

	private void UpdateRecommendedMarks()
	{
		List<StatType> selectedCareerRecommendedStats = CharGenUtility.GetSelectedCareerRecommendedStats<BlueprintSkillAdvancement>(Unit.Value);
		SetRecommendedMarks(selectedCareerRecommendedStats);
	}

	public void SetRecommendedMarks(List<StatType> recommendedSkills)
	{
		foreach (CharInfoStatVM stat in Stats)
		{
			stat.UpdateRecommendedMark(recommendedSkills);
		}
	}
}
