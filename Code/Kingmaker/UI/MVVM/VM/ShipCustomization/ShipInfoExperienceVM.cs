using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization;

public class ShipInfoExperienceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> CanLevelup = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> ShipExperience = new ReactiveProperty<string>();

	public readonly ReactiveProperty<float> ShipExperienceBar = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<int> NextLevelExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipLvl = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> AvailablePoints = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentLevelBar = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> Ranks = new ReactiveProperty<int>();

	private BlueprintStatProgression ExperienceTable => Game.Instance.BlueprintRoot.Progression.StarshipXPTable;

	public ShipInfoExperienceVM()
	{
		UpdateStats();
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateStats();
		}));
	}

	private void UpdateStats()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		ShipExperience.Value = playerShip.StarshipProgression.Progression.Experience + " / " + ExperienceTable.GetBonus(playerShip.StarshipProgression.Progression.ExperienceLevel + 1);
		ShipExperienceBar.Value = (float)playerShip.StarshipProgression.Progression.Experience / (float)ExperienceTable.GetBonus(playerShip.StarshipProgression.Progression.ExperienceLevel + 1);
		ShipLvl.Value = playerShip.StarshipProgression.Progression.ExperienceLevel;
		CurrentLevelBar.Value = Math.Abs(ShipLvl.Value - ExperienceTable.Bonuses[ExperienceTable.Bonuses.Length - 1]);
		NextLevelExp.Value = ExperienceTable.GetBonus(playerShip.StarshipProgression.Progression.CharacterLevel + 1);
		ShipLvl.Value = playerShip.StarshipProgression.Progression.ExperienceLevel;
		int characterLevel = playerShip.StarshipProgression.Progression.CharacterLevel;
		CanLevelup.Value = LevelUpController.CanLevelUp(playerShip);
		int experienceLevel = playerShip.StarshipProgression.Progression.ExperienceLevel;
		int value = Math.Max(0, experienceLevel - characterLevel);
		Ranks.Value = value;
	}

	protected override void DisposeImplementation()
	{
	}
}
