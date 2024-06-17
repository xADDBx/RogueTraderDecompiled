using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;

public class CharInfoAvailableRanksVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILevelUpManagerUIHandler, ISubscriber, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>
{
	public readonly ReactiveProperty<int> NextLevelExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentLevelExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> NewRanksCount = new ReactiveProperty<int>();

	private readonly CareerPathVM m_CareerPathVM;

	public bool IsInLevelupProcess => m_CareerPathVM.IsInLevelupProcess;

	public CharInfoAvailableRanksVM(CareerPathVM careerPathVM)
	{
		m_CareerPathVM = careerPathVM;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateData()
	{
		UpdateExp();
		UpdateLevel();
	}

	private void UpdateExp()
	{
		BaseUnitEntity unit = m_CareerPathVM.Unit;
		if (unit != null && !unit.IsDisposed)
		{
			BlueprintStatProgression xPTable = Game.Instance.BlueprintRoot.Progression.XPTable;
			NextLevelExp.Value = xPTable.GetBonus(unit.Progression.CharacterLevel + 1);
			CurrentLevelExp.Value = xPTable.GetBonus(unit.Progression.CharacterLevel);
			CurrentExp.Value = (unit.IsPet ? unit.Master.Progression.Experience : unit.Progression.Experience);
		}
	}

	private void UpdateLevel()
	{
		BaseUnitEntity unit = m_CareerPathVM.Unit;
		if (unit != null && !unit.IsDisposed)
		{
			int characterLevel = unit.Progression.CharacterLevel;
			int experienceLevel = unit.Progression.ExperienceLevel;
			int a = Math.Max(0, experienceLevel - characterLevel);
			a = Mathf.Min(a, m_CareerPathVM.MaxRank - m_CareerPathVM.CurrentRank.Value);
			NewRanksCount.Value = a;
		}
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		UpdateData();
	}

	public void HandleUICommitChanges()
	{
		UpdateData();
	}

	public void HandleUISelectionChanged()
	{
		UpdateData();
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateData();
	}
}
