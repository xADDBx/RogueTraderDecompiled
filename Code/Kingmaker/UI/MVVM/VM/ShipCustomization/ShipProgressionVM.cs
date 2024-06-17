using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization;

public class ShipProgressionVM : BaseUnitProgressionVM
{
	public readonly ShipInfoExperienceVM ShipInfoExperienceVM;

	public CareerPathVM CareerPathVM;

	public override CharInfoExperienceVM CharInfoExperienceVM => null;

	public override UnitBackgroundBlockVM UnitBackgroundBlockVM => null;

	public ShipProgressionVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
		AddDisposable(ShipInfoExperienceVM = new ShipInfoExperienceVM());
		BlueprintCareerPath shipPath = ProgressionRoot.Instance.ShipPath;
		AddDisposable(CareerPathVM = new CareerPathVM(Unit.Value, shipPath, this));
		SetCareerPath(CareerPathVM, force: true);
		CareerPathVM?.UpdateCareerPath();
	}

	protected sealed override void RefreshData()
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
		CareerPathVM.OnCommit?.Execute();
		DestroyLevelUpManager();
	}

	public override void SetPreviousState(bool saveSelections = false)
	{
	}

	public override void SelectCareerPath()
	{
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

	public sealed override void SetCareerPath(CareerPathVM careerPathVM, bool force = false)
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
		careerPathVM?.SetFirstSelectableRankEntry();
		CurrentCareer.Value = careerPathVM;
		careerPathVM?.UpdateState(updateRanks: true);
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
