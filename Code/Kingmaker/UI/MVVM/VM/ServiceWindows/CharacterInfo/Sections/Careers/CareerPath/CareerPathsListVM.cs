using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;

public class CareerPathsListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICareerPathHoverHandler, ISubscriber
{
	public readonly CareerPathTier Tier;

	public readonly string TierLabel;

	public readonly ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();

	public readonly List<CareerPathVM> CareerPathVMs;

	public readonly ReactiveProperty<CareerPathVM> PreviewCareer = new ReactiveProperty<CareerPathVM>();

	public readonly ReactiveProperty<CareerPathVM> SelectedCareer = new ReactiveProperty<CareerPathVM>();

	public readonly BoolReactiveProperty IsUnlocked = new BoolReactiveProperty();

	private readonly IReadOnlyReactiveProperty<CareerPathVM> m_PreselectedCareer;

	private CareerPathVM m_HoveredCareer;

	private static UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public CareerPathsListVM(CareerPathTier tier, List<CareerPathVM> careers, IReadOnlyReactiveProperty<CareerPathVM> preselectedCareer, List<BlueprintCareerPath> choosedCareers)
	{
		CareerPathsListVM careerPathsListVM = this;
		Tier = tier;
		m_PreselectedCareer = preselectedCareer;
		TierLabel = tier switch
		{
			CareerPathTier.One => Strings.CareersTier1.Text, 
			CareerPathTier.Two => Strings.CareersTier2.Text, 
			CareerPathTier.Three => Strings.CareersTier3.Text, 
			_ => string.Empty, 
		};
		CareerPathVMs = careers;
		BuildCareersPrerequisites();
		List<CareerPathVM> list = CareerPathVMs.Where((CareerPathVM vm) => vm.IsInProgress || vm.IsFinished || vm.PrerequisiteCareerPaths.FindIndex(choosedCareers.Contains) >= 0).ToList();
		if (list.Any())
		{
			CareerPathVMs = list;
		}
		AddDisposable(m_PreselectedCareer.Subscribe(delegate
		{
			careerPathsListVM.UpdatePreviewCareer();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	private void BuildCareersPrerequisites()
	{
		foreach (CareerPathVM careerPathVM in CareerPathVMs)
		{
			List<BlueprintCareerPath> list = new List<BlueprintCareerPath>();
			CalculatedPrerequisite prerequisite = careerPathVM.Prerequisite;
			GetPrerequisitesCareers(prerequisite, list);
			careerPathVM.PrerequisiteCareerPaths = list;
		}
	}

	private void GetPrerequisitesCareers(CalculatedPrerequisite prerequisite, List<BlueprintCareerPath> result)
	{
		if (prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact)
		{
			if (calculatedPrerequisiteFact.Fact is BlueprintCareerPath)
			{
				result.Add(calculatedPrerequisiteFact.Fact as BlueprintCareerPath);
			}
		}
		else
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite { Prerequisites: var prerequisites }))
			{
				return;
			}
			foreach (CalculatedPrerequisite item in prerequisites)
			{
				if (item is CalculatedPrerequisiteFact calculatedPrerequisiteFact2 && calculatedPrerequisiteFact2.Fact is BlueprintCareerPath)
				{
					result.Add(calculatedPrerequisiteFact2.Fact as BlueprintCareerPath);
				}
				else
				{
					GetPrerequisitesCareers(item, result);
				}
			}
		}
	}

	protected override void DisposeImplementation()
	{
		CareerPathVMs.Clear();
	}

	public void UpdateState()
	{
		foreach (CareerPathVM careerPathVM in CareerPathVMs)
		{
			careerPathVM.UpdateState(updateRanks: false);
			if (careerPathVM.IsAvailableToUpgrade || careerPathVM.IsSelectedAndInProgress)
			{
				IsActive.Value = true;
			}
			if (careerPathVM.IsSelectedAndInProgress || careerPathVM.IsFinished)
			{
				careerPathVM.UpdateState(updateRanks: true);
				SelectedCareer.Value = careerPathVM;
			}
		}
		IsUnlocked.Value = CareerPathVMs.Any((CareerPathVM path) => path.IsUnlocked);
	}

	public void HandleHoverStart(BlueprintCareerPath careerPath)
	{
		CareerPathVM hoveredCareer = CareerPathVMs.FirstOrDefault((CareerPathVM i) => i.CareerPath == careerPath);
		m_HoveredCareer = hoveredCareer;
		UpdatePreviewCareer();
	}

	public void HandleHoverStop()
	{
		m_HoveredCareer = null;
		UpdatePreviewCareer();
	}

	private void UpdatePreviewCareer()
	{
		CareerPathVM careerPathVM = CareerPathVMs.FirstOrDefault((CareerPathVM i) => i == m_PreselectedCareer.Value);
		PreviewCareer.Value = m_HoveredCareer ?? careerPathVM;
	}

	public int GetLevelToUnlock()
	{
		int num = 1;
		int i;
		for (i = 1; i <= (int)Tier; i++)
		{
			BlueprintCareerPath blueprintCareerPath = ProgressionRoot.Instance.CareerPaths.FirstOrDefault((BlueprintCareerPath path) => path.Tier == (CareerPathTier)(i - 1));
			if (blueprintCareerPath != null)
			{
				num += blueprintCareerPath.Ranks;
			}
		}
		return num;
	}
}
