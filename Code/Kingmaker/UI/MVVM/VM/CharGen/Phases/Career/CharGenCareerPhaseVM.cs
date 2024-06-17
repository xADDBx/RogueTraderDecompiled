using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Components;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Career;

public class CharGenCareerPhaseVM : CharGenPhaseBaseVM, ICareerPathHoverHandler, ISubscriber, ICharGenCareerPathHandler
{
	public UnitProgressionVM UnitProgressionVM;

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly Dictionary<BlueprintPath, FeatureSelectionItem> m_CareerPathToSelectionItem = new Dictionary<BlueprintPath, FeatureSelectionItem>();

	private SelectionStateFeature m_SelectionStateFeature;

	private bool m_Subscribed;

	public CharGenCareerPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Career)
	{
		base.HasPantograph = false;
		base.ShowVisualSettings.Value = false;
		base.PhaseNextHint.Value = UIStrings.Instance.CharGen.SelectDoctrineHint;
		CreateTooltipSystem();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override bool CheckIsCompleted()
	{
		SelectionStateFeature selectionStateFeature = m_SelectionStateFeature;
		if (selectionStateFeature != null && selectionStateFeature.IsMade)
		{
			return selectionStateFeature.IsValid;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		if (!m_Subscribed)
		{
			UnitProgressionVM = AddDisposableAndReturn(new UnitProgressionVM(CharGenContext.CurrentUnit, m_LevelUpManager, UnitProgressionMode.CharGen));
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			AddDisposable(UnitProgressionVM.PreselectedCareer.Subscribe(HandleSelectCareer));
			m_Subscribed = true;
		}
		UnitProgressionVM.UpdateSelectionsFromUnit(CharGenContext.LevelUpManager.Value.PreviewUnit);
		SetupDefaultItemsState();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_CareerPathToSelectionItem.Clear();
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	public void UpdateTooltipTemplate(BlueprintCareerPath careerPath = null)
	{
		if (careerPath == null)
		{
			m_ReactiveTooltipTemplate.Value = GetTooltipTemplate();
			return;
		}
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM vm) => vm.CareerPath == careerPath);
		m_ReactiveTooltipTemplate.Value = ((careerPathVM != null) ? ((TooltipBaseTemplate)new TooltipTemplateCareer(careerPathVM)) : ((TooltipBaseTemplate)new TooltipTemplateCharGenDoctrinesDesc()));
	}

	private TooltipBaseTemplate GetTooltipTemplate()
	{
		CareerPathVM value = UnitProgressionVM.PreselectedCareer.Value;
		if (value == null)
		{
			return new TooltipTemplateCharGenDoctrinesDesc();
		}
		return new TooltipTemplateCareer(value);
	}

	private void HandleSelectCareer(CareerPathVM careerPathVM)
	{
		if (careerPathVM != null && m_SelectionStateFeature != null)
		{
			Game.Instance.GameCommandQueue.CharGenSelectCareerPath(careerPathVM.CareerPath);
		}
	}

	void ICharGenCareerPathHandler.HandleCareerPath(BlueprintCareerPath careerPath)
	{
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM vm) => vm.CareerPath == careerPath);
		if (careerPathVM == null)
		{
			PFLog.UI.Error("CareerPathVM not found " + careerPath.AssetGuid);
			return;
		}
		UnitProgressionVM.PreselectedCareer.Value = careerPathVM;
		if (m_CareerPathToSelectionItem.TryGetValue(careerPathVM.CareerPath, out var value))
		{
			m_SelectionStateFeature.ClearSelection();
			m_SelectionStateFeature.Select(value);
		}
		UpdateIsCompleted();
		UpdateCareerPathsHighlight(careerPathVM);
		base.PhaseNextHint.Value = string.Empty;
		UpdateTooltipTemplate();
	}

	private void UpdateCareerPathsHighlight(CareerPathVM preselectedCareer)
	{
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager == null)
		{
			return;
		}
		IEnumerable<BlueprintSelectionFeature> featureSelectionsByGroup = CharGenUtility.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup.ChargenCareerPath, manager.PreviewUnit);
		if (!featureSelectionsByGroup.Any())
		{
			return;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = featureSelectionsByGroup.First();
		foreach (FeatureSelectionItem selectionItem in blueprintSelectionFeature.GetSelectionItems(manager.PreviewUnit, manager.Path))
		{
			ApplyCareerPath component = selectionItem.Feature.GetComponent<ApplyCareerPath>();
			if (component != null)
			{
				m_CareerPathToSelectionItem[component.CareerPath] = selectionItem;
			}
		}
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, blueprintSelectionFeature, 0) as SelectionStateFeature;
	}

	public void HandleHoverStart(BlueprintCareerPath careerPath)
	{
		if (UnitProgressionVM == null)
		{
			return;
		}
		bool flag = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM c) => c.CareerPath == careerPath)?.IsUnlocked ?? false;
		foreach (CareerPathVM allCareerPath in UnitProgressionVM.AllCareerPaths)
		{
			if (allCareerPath.PrerequisiteCareerPaths.Contains(careerPath))
			{
				allCareerPath.ItemState.Value = CareerItemState.Highlighted;
			}
			else if (flag)
			{
				if (allCareerPath.CareerPath.Tier == careerPath.Tier && allCareerPath.CareerPath != careerPath)
				{
					allCareerPath.ItemState.Value = CareerItemState.Darkened;
				}
				else
				{
					allCareerPath.ItemState.Value = (allCareerPath.IsUnlocked ? CareerItemState.Unlocked : CareerItemState.Locked);
				}
			}
		}
	}

	public void HandleHoverStop()
	{
		SetupDefaultItemsState();
	}

	private void SetupDefaultItemsState()
	{
		if (UnitProgressionVM == null)
		{
			return;
		}
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM c) => c.IsSelected.Value);
		foreach (CareerPathVM allCareerPath in UnitProgressionVM.AllCareerPaths)
		{
			if (careerPathVM != null && allCareerPath.CareerPath == careerPathVM.CareerPath)
			{
				allCareerPath.ItemState.Value = CareerItemState.Selected;
			}
			else if (careerPathVM == null && allCareerPath.CareerPath.Tier == CareerPathTier.One)
			{
				allCareerPath.ItemState.Value = CareerItemState.Ready;
			}
			else
			{
				allCareerPath.ItemState.Value = (allCareerPath.IsUnlocked ? CareerItemState.Unlocked : CareerItemState.Locked);
			}
		}
	}
}
