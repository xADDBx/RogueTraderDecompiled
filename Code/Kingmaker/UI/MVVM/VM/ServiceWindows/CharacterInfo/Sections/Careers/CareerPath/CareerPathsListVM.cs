using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;

public class CareerPathsListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICareerPathHoverHandler, ISubscriber
{
	public readonly CareerPathTier Tier;

	public readonly List<CareerPathVM> CareerPathVMs;

	public readonly List<CareerPathVM> UnlockingCareersVMs = new List<CareerPathVM>();

	public readonly ReactiveProperty<CareerPathVM> PreviewCareer = new ReactiveProperty<CareerPathVM>();

	public readonly ReactiveProperty<CareerPathVM> SelectedCareer = new ReactiveProperty<CareerPathVM>();

	public readonly BoolReactiveProperty IsUnlocked = new BoolReactiveProperty();

	private readonly IReadOnlyReactiveProperty<CareerPathVM> m_PreselectedCareer;

	private CareerPathVM m_HoveredCareer;

	public CareerPathsListVM(CareerPathTier tier, List<CareerPathVM> careers, IReadOnlyReactiveProperty<CareerPathVM> preselectedCareer, List<BlueprintCareerPath> choosedCareers)
	{
		CareerPathsListVM careerPathsListVM = this;
		Tier = tier;
		m_PreselectedCareer = preselectedCareer;
		CareerPathVMs = careers;
		BuildCareersPrerequisites();
		BuildUnlockingCareers();
		List<CareerPathVM> list = CareerPathVMs.Where((CareerPathVM vm) => (vm.IsInProgress || vm.IsFinished || vm.PrerequisiteCareerPaths.FindIndex(choosedCareers.Contains) >= 0 || vm.HasPrerequisiteFeatures() || !choosedCareers.Any()) && (!vm.CareerPath.HideNotAvailibleInUI || vm.Prerequisite.Value || vm.IsUnlocked)).ToList();
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
			List<BlueprintFeature> list2 = new List<BlueprintFeature>();
			CalculatedPrerequisite prerequisite = careerPathVM.Prerequisite;
			GetPrerequisitesCareers(prerequisite, list, list2);
			careerPathVM.PrerequisiteCareerPaths = list;
			careerPathVM.PrerequisiteFeatures = list2;
		}
	}

	private void BuildUnlockingCareers()
	{
		UnlockingCareersVMs.Add(CareerPathVMs.ElementAt(0));
	}

	private void GetPrerequisitesCareers(CalculatedPrerequisite prerequisite, List<BlueprintCareerPath> careerPaths, List<BlueprintFeature> features)
	{
		if (!(prerequisite is CalculatedPrerequisiteFact { Fact: var fact }))
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite { Prerequisites: var prerequisites }))
			{
				return;
			}
			{
				foreach (CalculatedPrerequisite item5 in prerequisites)
				{
					if (item5 is CalculatedPrerequisiteFact { Fact: var fact2 })
					{
						if (!(fact2 is BlueprintCareerPath item))
						{
							if (fact2 is BlueprintFeature item2)
							{
								features.Add(item2);
							}
						}
						else
						{
							careerPaths.Add(item);
						}
					}
					else
					{
						GetPrerequisitesCareers(item5, careerPaths, features);
					}
				}
				return;
			}
		}
		if (!(fact is BlueprintCareerPath item3))
		{
			if (fact is BlueprintFeature item4)
			{
				features.Add(item4);
			}
		}
		else
		{
			careerPaths.Add(item3);
		}
	}

	protected override void DisposeImplementation()
	{
		CareerPathVMs.Clear();
	}

	public void UpdateState()
	{
		UpdateCareers();
		IsUnlocked.Value = CareerPathVMs.Any((CareerPathVM path) => path.IsUnlocked || path.CanShowToAnotherCoopPlayer());
		if (Game.Instance.RootUiContext.MainMenuVM != null && Game.Instance.RootUiContext.MainMenuVM.CharGenContextVM != null)
		{
			IsUnlocked.Value = IsUnlocked.Value || (Game.Instance.RootUiContext.MainMenuVM.CharGenContextVM.CharGenVM.HasValue && Tier == CareerPathTier.One);
		}
		void UpdateCareers()
		{
			if (SelectedCareer.Value != null)
			{
				SelectedCareer.Value.UpdateState(updateRanks: true);
				return;
			}
			foreach (CareerPathVM careerPathVM in CareerPathVMs)
			{
				if (careerPathVM.IsSelectedAndInProgress || careerPathVM.IsFinished)
				{
					careerPathVM.UpdateState(updateRanks: true);
					SelectedCareer.Value = careerPathVM;
					break;
				}
				careerPathVM.UpdateState(updateRanks: false);
			}
		}
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
