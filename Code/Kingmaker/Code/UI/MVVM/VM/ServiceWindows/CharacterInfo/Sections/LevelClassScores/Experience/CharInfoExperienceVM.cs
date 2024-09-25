using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;

public class CharInfoExperienceVM : CharInfoComponentVM, ILevelUpManagerUIHandler, ISubscriber, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>
{
	public readonly ReactiveProperty<int> NextLevelExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentLevelExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> CurrentLevelExpRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<bool> CanLevelup = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> Level = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> HasPsyRating = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> PsyRating = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> NewRanksCount = new ReactiveProperty<int>();

	public TooltipBaseTemplate PsyRatingTooltip;

	public CharInfoExperienceVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		if (Unit.Value != null)
		{
			UpdateData();
		}
	}

	private void UpdateData()
	{
		UpdateExp();
		UpdateLevel();
		UpdatePsyRating();
	}

	private void UpdateExp()
	{
		BlueprintStatProgression xPTable = Game.Instance.BlueprintRoot.Progression.XPTable;
		NextLevelExp.Value = xPTable.GetBonus(Unit.Value.Progression.CharacterLevel + 1);
		CurrentLevelExp.Value = xPTable.GetBonus(Unit.Value.Progression.CharacterLevel);
		CurrentExp.Value = (Unit.Value.IsPet ? Unit.Value.Master.Progression.Experience : Unit.Value.Progression.Experience);
		int num = CurrentExp.Value - CurrentLevelExp.Value;
		int num2 = NextLevelExp.Value - CurrentLevelExp.Value;
		CurrentLevelExpRatio.Value = ((num2 > 0) ? ((float)num / (float)num2) : 0f);
	}

	private void UpdateLevel()
	{
		int characterLevel = Unit.Value.Progression.CharacterLevel;
		Level.Value = characterLevel;
		CanLevelup.Value = LevelUpController.CanLevelUp(Unit.Value);
		int experienceLevel = Unit.Value.Progression.ExperienceLevel;
		int value = Math.Max(0, experienceLevel - characterLevel);
		NewRanksCount.Value = value;
	}

	private void UpdatePsyRating()
	{
		ModifiableValue statOptional = Unit.Value.GetStatOptional(StatType.PsyRating);
		if (statOptional != null)
		{
			PsyRating.Value = statOptional.ModifiedValue;
			PsyRatingTooltip = new TooltipTemplateStat(new StatTooltipData(statOptional));
		}
		HasPsyRating.Value = statOptional != null;
	}

	public void LevelUp()
	{
		EventBus.RaiseEvent(delegate(ILevelUpInitiateUIHandler h)
		{
			h.HandleLevelUpStart(Unit.Value);
		});
	}

	public new void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public new void HandleDestroyLevelUpManager()
	{
	}

	public new void HandleUISelectCareerPath()
	{
		UpdateData();
	}

	public new void HandleUICommitChanges()
	{
		UpdateData();
	}

	public new void HandleUISelectionChanged()
	{
		UpdateData();
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateData();
	}
}
