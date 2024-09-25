using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization;

public sealed class ShipProgressionVM : BaseUnitProgressionVM
{
	public readonly ShipInfoExperienceVM ShipInfoExperienceVM;

	public CareerPathVM CareerPathVM;

	public ShipProgressionVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
		AddDisposable(ShipInfoExperienceVM = new ShipInfoExperienceVM());
		BlueprintCareerPath shipPath = ProgressionRoot.Instance.ShipPath;
		AddDisposable(CareerPathVM = new CareerPathVM(Unit.Value, shipPath, this));
		SetCareerPath(CareerPathVM, force: true);
	}

	protected override void RefreshData()
	{
		if (Unit.Value != null)
		{
			CurrentRankEntryItem.Value = null;
			CareerPathVM?.UpdateCareerPath();
		}
	}

	public override void Commit()
	{
		Game.Instance.Player.PlayerShip.StarshipProgression.AddStarshipLevel(m_LevelUpManager.Value);
		DestroyLevelUpManager();
	}

	private void TrySaveState()
	{
		Game.Instance.Player.UISettings.SavedUnitProgressionWindowData.CareerPath = CareerPathVM.CareerPath.ToReference<BlueprintCareerPath.Reference>();
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
		if (Unit.Value.Progression.CanUpgradePath(careerPathVM?.CareerPath))
		{
			CreateLevelUpManager(careerPathVM?.CareerPath);
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleUISelectCareerPath();
			});
			AddDisposable(careerPathVM?.CurrentProgress.Subscribe(delegate
			{
				FirstAvailableEntryItem.Value = careerPathVM.FirstSelectable;
			}));
		}
		CurrentCareer.Value = careerPathVM;
		careerPathVM?.UpdateState(updateRanks: true);
		careerPathVM?.RefreshTooltipUnit();
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(SetFirstAvailableRankEntry);
	}

	public void SetFirstAvailableRankEntry()
	{
		if (CurrentCareer.Value == null)
		{
			return;
		}
		RankEntrySelectionVM rankEntrySelectionVM = CurrentCareer.Value.AllSelections?.LastOrDefault((RankEntrySelectionVM s) => s.SelectionMade);
		if (CurrentCareer.Value.IsInLevelupProcess)
		{
			SetRankEntry(rankEntrySelectionVM);
			if (CurrentCareer.Value.LastEntryToUpgrade == rankEntrySelectionVM)
			{
				CurrentCareer.Value.SetRankEntry(rankEntrySelectionVM);
			}
			else
			{
				CurrentCareer.Value.SelectNextItem(skipSelected: false);
			}
		}
		else
		{
			CurrentCareer.Value.SetRankEntry(null);
		}
	}

	private void DestroyLevelUpManager()
	{
		if (m_LevelUpManager?.Value != null)
		{
			m_LevelUpManager.Value.Dispose();
			m_LevelUpManager.Value = null;
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleDestroyLevelUpManager();
			});
		}
	}

	private void CreateLevelUpManager(BlueprintCareerPath careerPath)
	{
		if (m_LevelUpManager != null)
		{
			LevelUpManager manager = new LevelUpManager(Unit.Value, careerPath, autoCommit: false);
			m_LevelUpManager.Value = manager;
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleCreateLevelUpManager(manager);
			});
		}
	}

	protected override void DisposeImplementation()
	{
		TrySaveState();
		DestroyLevelUpManager();
		CareerPathVM?.Dispose();
		CurrentCareer.Value = null;
	}
}
