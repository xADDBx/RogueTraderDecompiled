using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class ShipVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISelectionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IGameModeHandler
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> ShipMilitaryRating = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> ShipTurretRating = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> ShipArmorFront = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> ShipArmorLeft = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> ShipArmorRight = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> ShipArmorRear = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> ShipShieldValue = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> ShipName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> ShipMorale = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> ShipMoraleValue = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> ShipCrew = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> ShipCrewValue = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<int> ShipFrontShield = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShipRearShield = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShipLeftShield = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShipRightShield = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ProwRamDamageBonus = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ProwRamSelfDamageReduceBonus = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<string> ShipExperience = new ReactiveProperty<string>();

	public readonly ReactiveProperty<float> ShipExperienceBar = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<int> ShipLvl = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<ShipCustomizationTab> m_CurrentTab = new ReactiveProperty<ShipCustomizationTab>();

	private BlueprintStatProgression ExperienceTable => Game.Instance.BlueprintRoot.Progression.StarshipXPTable;

	public ShipVM(ReactiveProperty<ShipCustomizationTab> shipCustomizationTab = null)
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
		if (shipCustomizationTab != null)
		{
			AddDisposable(shipCustomizationTab.Subscribe(delegate(ShipCustomizationTab val)
			{
				m_CurrentTab.Value = val;
			}));
		}
	}

	protected override void DisposeImplementation()
	{
	}

	private void OnUpdateHandler()
	{
		UpdateStats();
	}

	public void OnUnitSelectionAdd(bool single, bool ask)
	{
		UpdateStats();
	}

	public void OnUnitSelectionRemove()
	{
	}

	private void UpdateStats()
	{
		StarshipEntity ship = Game.Instance.Player.PlayerShip;
		PartStarshipHull hull = ship.Hull;
		PartStarshipShields shields = ship.Shields;
		ShipName.Value = ship.CharacterName;
		ShipShieldValue.Value = "0/0";
		ShipTurretRating.Value = ship.Stats.GetStat(StatType.TurretRating).ModifiedValue.ToString();
		ShipMilitaryRating.Value = ship.GetHull().CurrentMilitaryRating.ToString();
		ShipArmorFront.Value = AggregateArmorSources(StatType.ArmourFore, (BlueprintItemArmorPlating ap) => ap?.ArmourFore, (StarshipArmorBonus ab) => ab.ArmourFore);
		ShipArmorLeft.Value = AggregateArmorSources(StatType.ArmourPort, (BlueprintItemArmorPlating ap) => ap?.ArmourPort, (StarshipArmorBonus ab) => ab.ArmourPort);
		ShipArmorRight.Value = AggregateArmorSources(StatType.ArmourStarboard, (BlueprintItemArmorPlating ap) => ap?.ArmourStarboard, (StarshipArmorBonus ab) => ab.ArmourStarboard);
		ShipArmorRear.Value = AggregateArmorSources(StatType.ArmourAft, (BlueprintItemArmorPlating ap) => ap?.ArmourAft, (StarshipArmorBonus ab) => ab.ArmourAft);
		ShipMorale.Value = (float)ship.Morale.MoraleLeft / (float)ship.Morale.MaxMorale;
		if (ship.Morale.MaxMorale != 0)
		{
			ShipMoraleValue.Value = (float)ship.Morale.MoraleLeft / (float)ship.Morale.MaxMorale * 100f + "%";
		}
		ShipFrontShield.Value = shields.GetShields(StarshipSectorShieldsType.Fore).Current;
		ShipRearShield.Value = shields.GetShields(StarshipSectorShieldsType.Aft).Current;
		ShipLeftShield.Value = shields.GetShields(StarshipSectorShieldsType.Port).Current;
		ShipRightShield.Value = shields.GetShields(StarshipSectorShieldsType.Starboard).Current;
		ShipExperience.Value = ship.StarshipProgression.Progression.Experience + " / " + ExperienceTable.GetBonus(ship.StarshipProgression.Progression.ExperienceLevel + 1);
		ShipExperienceBar.Value = (float)ship.StarshipProgression.Progression.Experience / (float)ExperienceTable.GetBonus(ship.StarshipProgression.Progression.ExperienceLevel + 1);
		ShipArmorRear.Value = (int)ship.Stats.GetStat(StatType.ArmourAft) + (ship.ArmorPlatings?.ArmourAft ?? 0);
		ProwRamDamageBonus.Value = hull.ProwRam.BonusDamage;
		ProwRamSelfDamageReduceBonus.Value = hull.ProwRam.SelfDamageDeduction;
		ShipLvl.Value = ship.StarshipProgression.Progression.ExperienceLevel;
		ShipCrew.Value = ship.Crew.Ratio;
		ShipCrewValue.Value = ship.Crew.Count + "k";
		int AggregateArmorSources(StatType statType, Func<BlueprintItemArmorPlating, int?> fItemArmor, Func<StarshipArmorBonus, int> fBonusArmor)
		{
			int num = ship.Stats.GetStat(StatType.ArmourFore);
			int num2 = ship.Facts.GetComponents<StarshipArmorBonus>().Select(fBonusArmor).Aggregate(fItemArmor(ship.ArmorPlatings).GetValueOrDefault(), (int a, int b) => a + b);
			return num + num2;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		ShouldShow.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat || (RootUIContext.Instance.IsShipInventoryShown && m_CurrentTab.Value == ShipCustomizationTab.Upgrade);
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
