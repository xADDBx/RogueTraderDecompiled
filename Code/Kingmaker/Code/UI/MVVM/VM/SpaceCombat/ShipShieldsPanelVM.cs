using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class ShipShieldsPanelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<float> ShipHealthRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<string> ShipHealthText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> ShipArmorFore = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipArmorStarboard = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipArmorPort = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipArmorAft = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipShieldsFore = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipShieldsStarboard = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipShieldsPort = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ShipShieldsAft = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> ShipShieldsForeRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> ShipShieldsStarboardRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> ShipShieldsPortRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> ShipShieldsAftRatio = new ReactiveProperty<float>();

	private readonly Dictionary<StarshipSectorShieldsType, ShieldProps> m_ShieldsProps;

	public ShipShieldsPanelVM()
	{
		m_ShieldsProps = new Dictionary<StarshipSectorShieldsType, ShieldProps>
		{
			{
				StarshipSectorShieldsType.Fore,
				new ShieldProps
				{
					Current = ShipShieldsFore,
					Ratio = ShipShieldsForeRatio
				}
			},
			{
				StarshipSectorShieldsType.Starboard,
				new ShieldProps
				{
					Current = ShipShieldsStarboard,
					Ratio = ShipShieldsStarboardRatio
				}
			},
			{
				StarshipSectorShieldsType.Port,
				new ShieldProps
				{
					Current = ShipShieldsPort,
					Ratio = ShipShieldsPortRatio
				}
			},
			{
				StarshipSectorShieldsType.Aft,
				new ShieldProps
				{
					Current = ShipShieldsAft,
					Ratio = ShipShieldsAftRatio
				}
			}
		};
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
	}

	protected override void DisposeImplementation()
	{
		m_ShieldsProps.Clear();
	}

	private void UpdateHandler()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		ShipHealthRatio.Value = (float)playerShip.Health.HitPointsLeft / (float)playerShip.Health.MaxHitPoints;
		ShipHealthText.Value = $"{playerShip.Health.HitPointsLeft}/{playerShip.Health.MaxHitPoints}";
		ShipArmorFore.Value = playerShip.Stats.GetStat(StatType.ArmourFore).ModifiedValue;
		ShipArmorStarboard.Value = playerShip.Stats.GetStat(StatType.ArmourStarboard).ModifiedValue;
		ShipArmorPort.Value = playerShip.Stats.GetStat(StatType.ArmourPort).ModifiedValue;
		ShipArmorAft.Value = playerShip.Stats.GetStat(StatType.ArmourAft).ModifiedValue;
		PartStarshipShields starshipShieldsOptional = playerShip.GetStarshipShieldsOptional();
		if (starshipShieldsOptional == null)
		{
			return;
		}
		foreach (KeyValuePair<StarshipSectorShieldsType, ShieldProps> shieldsProp in m_ShieldsProps)
		{
			shieldsProp.Deconstruct(out var key, out var value);
			StarshipSectorShieldsType sector = key;
			ShieldProps shieldProps = value;
			StarshipSectorShields shields = starshipShieldsOptional.GetShields(sector);
			shieldProps.Current.Value = shields.Current;
			shieldProps.Ratio.Value = (float)shields.Current / (float)shields.Max;
		}
	}
}
