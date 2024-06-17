using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;

public abstract class CharGenBackgroundBasePhaseVM<TViewModel> : CharGenPhaseBaseVM, ICharGenSelectItemHandler, ISubscriber where TViewModel : CharGenBackgroundBaseItemVM
{
	public readonly SelectionGroupRadioVM<TViewModel> SelectionGroup;

	public readonly ReactiveProperty<TViewModel> SelectedItem = new ReactiveProperty<TViewModel>();

	protected readonly FeatureGroup FeatureGroup;

	protected readonly ReactiveCollection<TViewModel> Items = new ReactiveCollection<TViewModel>();

	protected readonly ReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly BoolReactiveProperty CurrentPageIsFirst = new BoolReactiveProperty();

	public readonly BoolReactiveProperty CurrentPageIsLast = new BoolReactiveProperty();

	protected bool Subscribed;

	protected bool CanShowVisualSettings = true;

	private BlueprintSelectionFeature m_Selection;

	private SelectionStateFeature m_SelectionStateFeature;

	private IDisposable m_DelayedApplySelection;

	protected Action OnSelectionApplied;

	public readonly ReactiveProperty<CharGenPhaseBaseVM> CurrentPhase;

	protected CharGenBackgroundBasePhaseVM(CharGenContext charGenContext, FeatureGroup featureGroup, CharGenPhaseType phaseType, ReactiveProperty<CharGenPhaseBaseVM> currentPhase = null)
		: base(charGenContext, phaseType)
	{
		CurrentPhase = currentPhase;
		FeatureGroup = featureGroup;
		SelectionGroup = new SelectionGroupRadioVM<TViewModel>(Items, SelectedItem);
		AddDisposable(SelectionGroup);
		AddDisposable(SelectedItem.Subscribe(HandleSelectedItem));
		AddDisposable(SelectedItem.Subscribe(delegate(TViewModel value)
		{
			CurrentPageIsFirst.Value = Items.FirstOrDefault() == value;
			CurrentPageIsLast.Value = Items.LastOrDefault() == value;
		}));
		CreateTooltipSystem();
		TrySelectItem();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
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
		if (!Subscribed)
		{
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			Subscribed = true;
		}
		else
		{
			RefreshItems(CharGenContext.LevelUpManager.Value);
			RefreshVisualSettings();
		}
		TrySelectItem();
	}

	protected virtual void Clear()
	{
		if (m_DelayedApplySelection != null)
		{
			m_DelayedApplySelection.Dispose();
			m_DelayedApplySelection = null;
			ApplySelection();
		}
		Items.ForEach(delegate(TViewModel vm)
		{
			vm.Dispose();
		});
		Items.Clear();
		m_SelectionStateFeature?.ClearSelection();
		SelectedItem.Value = null;
	}

	protected virtual void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		IEnumerable<BlueprintSelectionFeature> featureSelectionsByGroup = CharGenUtility.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup, manager.PreviewUnit);
		if (!featureSelectionsByGroup.Any())
		{
			return;
		}
		m_Selection = featureSelectionsByGroup.First();
		List<FeatureSelectionItem> list = m_Selection.GetSelectionItems(manager.PreviewUnit, manager.Path).ToList();
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, m_Selection, 0) as SelectionStateFeature;
		list.RemoveAll(delegate(FeatureSelectionItem i)
		{
			if (m_SelectionStateFeature != null)
			{
				CalculatedPrerequisite calculatedPrerequisite = m_SelectionStateFeature.GetCalculatedPrerequisite(i);
				if (calculatedPrerequisite != null)
				{
					return !calculatedPrerequisite.Value;
				}
				return false;
			}
			return true;
		});
		foreach (FeatureSelectionItem item in list)
		{
			Items.Add(CreateItem(item, m_SelectionStateFeature, PhaseType));
		}
		UpdateIsCompleted();
	}

	private void RefreshItems(LevelUpManager manager)
	{
		List<FeatureSelectionItem> selectionItems = m_Selection.GetSelectionItems(manager.PreviewUnit, manager.Path).ToList();
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, m_Selection, 0) as SelectionStateFeature;
		selectionItems.RemoveAll(delegate(FeatureSelectionItem i)
		{
			CalculatedPrerequisite calculatedPrerequisite = m_SelectionStateFeature.GetCalculatedPrerequisite(i);
			return calculatedPrerequisite != null && !calculatedPrerequisite.Value;
		});
		Items.RemoveAll((TViewModel i) => !selectionItems.HasItem((FeatureSelectionItem newItem) => i.Feature == newItem.Feature));
		foreach (FeatureSelectionItem newItem in selectionItems)
		{
			if (!Items.HasItem((TViewModel i) => i.Feature == newItem.Feature))
			{
				Items.Add(CreateItem(newItem, m_SelectionStateFeature, PhaseType));
			}
		}
		if (!Items.Contains(SelectedItem.Value))
		{
			SelectionGroup.TrySelectFirstValidEntity();
		}
	}

	protected void RefreshVisualSettings()
	{
		if (CanShowVisualSettings)
		{
			CharGenUtility.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var colorPreset);
			base.ShowVisualSettings.Value = !CharGenContext.Doll.ShowCloth || colorPreset != null;
		}
		else
		{
			base.ShowVisualSettings.Value = false;
		}
	}

	protected void HandleSelectedItem(TViewModel item)
	{
		if (item != null && FeatureGroup != 0)
		{
			Game.Instance.GameCommandQueue.CharGenSelectItem(FeatureGroup, item.Feature);
		}
		else
		{
			DelayedApplySelection();
		}
	}

	void ICharGenSelectItemHandler.HandleSelectItem(FeatureGroup featureGroup, BlueprintFeature blueprintFeature)
	{
		if (FeatureGroup == featureGroup)
		{
			TViewModel value = ((blueprintFeature != null) ? Items.FirstOrDefault((TViewModel item) => blueprintFeature == item?.Feature) : null);
			SelectedItem.Value = value;
			DelayedApplySelection();
		}
	}

	public bool GoNextPage()
	{
		return SelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return SelectionGroup.SelectPrevValidEntity();
	}

	private void DelayedApplySelection()
	{
		m_DelayedApplySelection?.Dispose();
		m_DelayedApplySelection = DelayedInvoker.InvokeInFrames(ApplySelection, 1);
		SetupTooltipTemplate();
	}

	private void ApplySelection()
	{
		if (SelectedItem.Value != null)
		{
			SelectedItem.Value.ApplySelection();
			LevelUpManager value = CharGenContext.LevelUpManager.Value;
			if (value != null)
			{
				CharGenContext.Doll.UpdateMechanicsEntities(value.PreviewUnit);
				OnSelectionApplied?.Invoke();
			}
			RefreshVisualSettings();
			m_DelayedApplySelection = null;
			UpdateIsCompleted();
		}
	}

	protected void TrySelectItem()
	{
		if (SelectedItem.Value == null)
		{
			SelectionGroup.TrySelectFirstValidEntity();
		}
	}

	protected abstract TViewModel CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType);

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	protected void SetupTooltipTemplate()
	{
		ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	protected virtual TooltipBaseTemplate TooltipTemplate()
	{
		if (SelectedItem.Value == null)
		{
			return null;
		}
		return new TooltipTemplateChargenBackground(SelectedItem.Value.Feature);
	}
}
