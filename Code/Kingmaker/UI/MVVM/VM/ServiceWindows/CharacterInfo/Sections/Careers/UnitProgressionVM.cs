using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;

public class UnitProgressionVM : BaseUnitProgressionVM
{
	public readonly ReactiveProperty<UnitProgressionWindowState> State = new ReactiveProperty<UnitProgressionWindowState>(UnitProgressionWindowState.CareerPathList);

	public readonly ReactiveProperty<CareerPathVM> PreselectedCareer = new ReactiveProperty<CareerPathVM>();

	public readonly AutoDisposingList<CareerPathsListVM> CareerPathsList = new AutoDisposingList<CareerPathsListVM>();

	public readonly AutoDisposingReactiveCollection<ProgressionBreadcrumbsItemVM> Breadcrumbs = new AutoDisposingReactiveCollection<ProgressionBreadcrumbsItemVM>();

	private readonly ReactiveCollection<CareerPathVM> m_AllCareerPaths = new ReactiveCollection<CareerPathVM>();

	private IDisposable m_EscHandle;

	private bool m_RankEntryHasSelection;

	private readonly UnitProgressionMode m_ProgressionMode;

	public override CharInfoExperienceVM CharInfoExperienceVM { get; }

	public override UnitBackgroundBlockVM UnitBackgroundBlockVM { get; }

	public IEnumerable<CareerPathVM> AllCareerPaths => m_AllCareerPaths;

	public bool IsCharGen => m_ProgressionMode == UnitProgressionMode.CharGen;

	public UnitProgressionVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReactiveProperty<LevelUpManager> levelUpManager, UnitProgressionMode mode)
		: base(unit, levelUpManager)
	{
		m_ProgressionMode = mode;
		AddDisposable(CharInfoExperienceVM = new CharInfoExperienceVM(unit));
		AddDisposable(UnitBackgroundBlockVM = new UnitBackgroundBlockVM(unit));
		AddDisposable(new SelectionGroupRadioVM<CareerPathVM>(m_AllCareerPaths, PreselectedCareer));
		TryRestoreSavedState();
	}

	protected override void DisposeImplementation()
	{
		TrySaveState();
		DestroyLevelUpManager();
		Clear();
		m_EscHandle?.Dispose();
		m_EscHandle = null;
	}

	protected override void RefreshData()
	{
		Clear();
		if (Unit.Value == null)
		{
			return;
		}
		Dictionary<CareerPathTier, List<CareerPathVM>> dictionary = new Dictionary<CareerPathTier, List<CareerPathVM>>();
		foreach (BlueprintCareerPath careerPath in ProgressionRoot.Instance.CareerPaths)
		{
			if (!dictionary.ContainsKey(careerPath.Tier))
			{
				dictionary[careerPath.Tier] = new List<CareerPathVM>();
			}
			try
			{
				CareerPathVM item = new CareerPathVM(Unit.Value, careerPath, this, m_ProgressionMode == UnitProgressionMode.CharGen);
				dictionary[careerPath.Tier].Add(item);
				m_AllCareerPaths.Add(item);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		List<BlueprintCareerPath> list = Unit.Value.Facts.List.Select((EntityFact f) => f.Blueprint as BlueprintCareerPath).ToList();
		list.RemoveAll((BlueprintCareerPath c) => c == null);
		foreach (CareerPathTier value in Enum.GetValues(typeof(CareerPathTier)))
		{
			if (dictionary.ContainsKey(value))
			{
				CareerPathsListVM careerPathsListVM = new CareerPathsListVM(value, dictionary[value], PreselectedCareer, list);
				AddDisposable(careerPathsListVM);
				CareerPathsList.Add(careerPathsListVM);
			}
		}
		CurrentRankEntryItem.Value = null;
		SetCareerPath(TryGetActiveLevelupCareer(), force: true);
		TryGetActiveLevelupCareer()?.UpdateCareerPath();
	}

	public override void SetRankEntry(IRankEntrySelectItem rankEntryItem)
	{
		if (rankEntryItem == CurrentRankEntryItem.Value)
		{
			OnRepeatedCurrentRankEntryItem.Execute(rankEntryItem);
		}
		CurrentRankEntryItem.Value = rankEntryItem;
	}

	public override void SetCareerPath(CareerPathVM careerPathVM, bool force = false)
	{
		careerPathVM?.InitializeRankEntries();
		CareerPathVM oldCareer = (force ? null : CurrentCareer.Value);
		TrySelectCareerPath(oldCareer, careerPathVM);
		UpdateState();
	}

	public override void SelectCareerPath()
	{
		TrySelectCareerPath(null, CurrentCareer.Value);
	}

	public override void Commit()
	{
		Game.Instance.GameCommandQueue.CommitLvlUp(base.LevelUpManager);
		DestroyLevelUpManager();
	}

	public void UpdateSelectionsFromUnit(BaseUnitEntity unit)
	{
		UnitBackgroundBlockVM.UpdateSelectionsFromUnit(unit);
	}

	private void TrySelectCareerPath(CareerPathVM oldCareer, CareerPathVM newCareer)
	{
		if (m_ProgressionMode == UnitProgressionMode.CharGen || (base.LevelUpManager != null && base.LevelUpManager.TargetUnit != Unit.Value))
		{
			CurrentCareer.Value = newCareer;
			CareerPathVM careerPathVM = newCareer;
			if (careerPathVM != null && careerPathVM.CareerPath.Tier == CareerPathTier.One)
			{
				FirstAvailableEntryItem.Value = CurrentCareer.Value?.RankEntries.FirstOrDefault()?.GetFirstItem();
			}
			else
			{
				FirstAvailableEntryItem.Value = null;
			}
			return;
		}
		if (oldCareer != null)
		{
			if (oldCareer.AvailableSelections.Select((RankEntrySelectionVM i) => i.SelectedFeature.Value).Any((RankEntrySelectionFeatureVM i) => i != null))
			{
				string message = ((m_ProgressionMode == UnitProgressionMode.CharGen) ? UIStrings.Instance.CharacterSheet.DialogCloseProgression : UIStrings.Instance.CharacterSheet.LevelupDialogCloseProgression);
				CloseWithMessage(message, OnYes, null);
			}
			else
			{
				DestroyLevelUpManager();
				CurrentCareer.Value = newCareer;
			}
		}
		else
		{
			if (newCareer != null && newCareer.IsAvailableToUpgrade)
			{
				CreateLevelUpManager(newCareer.CareerPath);
				EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
				{
					h.HandleUISelectCareerPath();
				});
			}
			CurrentCareer.Value = newCareer;
			AddDisposable(CurrentCareer.Value?.CurrentProgress.Subscribe(delegate
			{
				FirstAvailableEntryItem.Value = CurrentCareer.Value?.FirstSelectable;
			}));
		}
		TrySaveState();
		void OnYes()
		{
			DestroyLevelUpManager();
			CurrentCareer.Value = newCareer;
			UpdateState();
		}
	}

	public void TryClose(Action onYes, Action onNo)
	{
		CareerPathVM value = CurrentCareer.Value;
		if (value != null && value.AvailableSelections.Select((RankEntrySelectionVM i) => i.SelectedFeature.Value).Any((RankEntrySelectionFeatureVM i) => i != null))
		{
			CloseWithMessage(UIStrings.Instance.CharacterSheet.LevelupDialogCloseProgression, onYes, onNo);
		}
		else
		{
			onYes?.Invoke();
		}
	}

	private void CloseWithMessage(string message, Action onYes, Action onNo)
	{
		UIUtility.ShowMessageBox(message, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				onYes?.Invoke();
			}
			else
			{
				onNo?.Invoke();
			}
		});
	}

	private CareerPathVM TryGetActiveLevelupCareer()
	{
		if (base.LevelUpManager == null || base.LevelUpManager.TargetUnit != Unit.Value)
		{
			return null;
		}
		return GetCareerPathByBlueprint(base.LevelUpManager.Path);
	}

	private CareerPathVM GetCareerPathByBlueprint(BlueprintPath path)
	{
		return CareerPathsList.SelectMany((CareerPathsListVM i) => i.CareerPathVMs).FirstOrDefault((CareerPathVM i) => i.CareerPath == path);
	}

	private void CreateLevelUpManager(BlueprintCareerPath careerPath)
	{
		LevelUpManager manager = new LevelUpManager(Unit.Value, careerPath, autoCommit: false);
		m_LevelUpManager.Value = manager;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleCreateLevelUpManager(manager);
		});
	}

	public void ClearLevelupManagerIfNeeded(BaseUnitEntity newUnitEntity)
	{
		if (base.LevelUpManager != null && base.LevelUpManager.TargetUnit != newUnitEntity)
		{
			DestroyLevelUpManager();
		}
	}

	private void DestroyLevelUpManager()
	{
		m_LevelUpManager.Value?.Dispose();
		m_LevelUpManager.Value = null;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleDestroyLevelUpManager();
		});
	}

	private void TryRestoreSavedState()
	{
		if (m_ProgressionMode == UnitProgressionMode.CharGen)
		{
			return;
		}
		SavedUnitProgressionWindowData savedData = Game.Instance.Player.UISettings.SavedUnitProgressionWindowData;
		CareerPathVM careerPathVM = null;
		if (savedData.CareerPath == null)
		{
			return;
		}
		foreach (CareerPathsListVM careerPaths in CareerPathsList)
		{
			careerPathVM = careerPaths.CareerPathVMs.FirstOrDefault((CareerPathVM careerPath) => careerPath.CareerPath == savedData.CareerPath.Get());
			if (careerPathVM != null)
			{
				break;
			}
		}
		careerPathVM?.SetCareerPath();
	}

	private void TrySaveState()
	{
		if (m_ProgressionMode != 0)
		{
			Game.Instance.Player.UISettings.SavedUnitProgressionWindowData.CareerPath = CurrentCareer.Value?.CareerPath.ToReference<BlueprintCareerPath.Reference>();
		}
	}

	private void SetState(UnitProgressionWindowState newState, bool saveSelections = false)
	{
		switch (newState)
		{
		case UnitProgressionWindowState.CareerPathList:
			SetCareerPath(null);
			break;
		}
		CurrentRankEntryItem.Value = null;
		UpdateState();
	}

	public override void SetPreviousState(bool saveSelections = false)
	{
		int num = Breadcrumbs.Count - 1;
		int index = Math.Max(0, num - 1);
		SetState(Breadcrumbs[index].ProgressionState, saveSelections);
	}

	private void UpdateState()
	{
		foreach (CareerPathsListVM careerPaths in CareerPathsList)
		{
			careerPaths.UpdateState();
		}
		UnitProgressionWindowState unitProgressionWindowState = ((CurrentCareer.Value != null) ? UnitProgressionWindowState.CareerPathProgression : UnitProgressionWindowState.CareerPathList);
		UpdateBreadcrumbs(unitProgressionWindowState);
		State.Value = unitProgressionWindowState;
	}

	private void UpdateBreadcrumbs(UnitProgressionWindowState newState)
	{
		Breadcrumbs.Clear();
		foreach (UnitProgressionWindowState value in Enum.GetValues(typeof(UnitProgressionWindowState)))
		{
			bool flag = newState == value;
			string text = value switch
			{
				UnitProgressionWindowState.CareerPathList => UIStrings.Instance.CharacterSheet.GetMenuLabel(CharInfoPageType.LevelProgression), 
				UnitProgressionWindowState.CareerPathProgression => CurrentCareer.Value?.Name, 
				_ => string.Empty, 
			};
			Breadcrumbs.Add(new ProgressionBreadcrumbsItemVM(value, text, flag, delegate(UnitProgressionWindowState s)
			{
				SetState(s);
			}));
			if (flag)
			{
				break;
			}
		}
		m_EscHandle?.Dispose();
		if (Breadcrumbs.Count - 1 > 0)
		{
			m_EscHandle = EscHotkeyManager.Instance.Subscribe(delegate
			{
				SetPreviousState();
			});
		}
	}

	private void Clear()
	{
		foreach (CareerPathVM allCareerPath in m_AllCareerPaths)
		{
			allCareerPath.Dispose();
		}
		m_AllCareerPaths.Clear();
		CareerPathsList.Clear();
		Breadcrumbs.Clear();
	}
}
