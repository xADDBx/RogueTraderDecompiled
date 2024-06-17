using System;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

[Serializable]
public class InternalStructure : SystemComponent, IHashable
{
	public bool IsMaxLevel => base.Blueprint.UpgradeCost.Length <= base.UpgradeLevel + 1;

	public bool IsEnoughScrap
	{
		get
		{
			if (!IsMaxLevel)
			{
				return base.Blueprint.UpgradeCost[base.UpgradeLevel + 1] <= (int)Game.Instance.Player.Scrap;
			}
			return false;
		}
	}

	public InternalStructure(StarshipEntity ship)
		: base(ship)
	{
	}

	public InternalStructure(JsonConstructorMark _)
		: base(_)
	{
	}

	public void RecalculateHealthBonus()
	{
		PartHealth partHealth = Owner?.GetRequired<PartHealth>();
		if (partHealth == null)
		{
			PFLog.Default.Error("Internal structure upgrade: Can't find Part Health");
			return;
		}
		ModifiableValue.Modifier modifier = partHealth.HitPoints.GetModifiers(ModifierDescriptor.ShipSystemComponent).FirstItem((ModifiableValue.Modifier x) => x.SourceStat == StatType.HitPoints);
		if (modifier == null)
		{
			partHealth.HitPoints.AddModifier(base.Blueprint.HealthBonus * base.UpgradeLevel, StatType.HitPoints, ModifierDescriptor.ShipSystemComponent);
		}
		else
		{
			modifier.ModValue = base.Blueprint.HealthBonus * base.UpgradeLevel;
		}
	}

	public override UpgradeResult Upgrade()
	{
		UpgradeResult result = UpgradeResult.Successful;
		if (base.Blueprint.UpgradeCost.Length < base.UpgradeLevel + 1)
		{
			result = UpgradeResult.MaxUpgrade;
		}
		int num = base.Blueprint.UpgradeCost[base.UpgradeLevel + 1];
		if (num > (int)Game.Instance.Player.Scrap)
		{
			result = UpgradeResult.NotEnoughScrap;
		}
		PartHealth partHealth = Game.Instance.Player.PlayerShip?.GetRequired<PartHealth>();
		if (partHealth == null)
		{
			PFLog.Default.Error("Internal structure upgrade: Can't find Part Health");
			result = UpgradeResult.Error;
		}
		if (result == UpgradeResult.Successful)
		{
			Game.Instance.Player.Scrap.Spend(num);
			ModifiableValue.Modifier modifier = partHealth?.HitPoints.GetModifiers(ModifierDescriptor.ShipSystemComponent).FirstItem((ModifiableValue.Modifier x) => x.SourceStat == StatType.HitPoints);
			if (modifier == null)
			{
				partHealth?.HitPoints.AddModifier(base.Blueprint.HealthBonus, StatType.HitPoints, ModifierDescriptor.ShipSystemComponent);
			}
			else
			{
				modifier.ModValue += base.Blueprint.HealthBonus;
				partHealth.HitPoints.UpdateValue();
			}
			base.UpgradeLevel++;
		}
		EventBus.RaiseEvent(delegate(IUpgradeSystemComponentHandler h)
		{
			h.HandleSystemComponentUpgrade(SystemComponentType.InternalStructure, result);
		});
		return result;
	}

	public override DowngradeResult Downgrade()
	{
		DowngradeResult result = DowngradeResult.Successful;
		if (base.UpgradeLevel == 0)
		{
			result = DowngradeResult.MinUpgrade;
		}
		PartHealth partHealth = Game.Instance.Player.PlayerShip?.GetRequired<PartHealth>();
		if (partHealth == null)
		{
			PFLog.Default.Error("Internal structure downgrade: Can't find Part Health");
			result = DowngradeResult.Error;
		}
		ModifiableValue.Modifier modifier = partHealth?.HitPoints.GetModifiers(ModifierDescriptor.ShipSystemComponent).FirstItem();
		if (modifier == null)
		{
			PFLog.Default.Error("Internal structure downgrade: No active health modifier");
			result = DowngradeResult.Error;
		}
		if (result == DowngradeResult.Successful)
		{
			base.UpgradeLevel--;
			int scrap = base.Blueprint.UpgradeCost[base.UpgradeLevel + 1];
			Game.Instance.Player.Scrap.Receive(scrap);
			modifier.ModValue -= base.Blueprint.HealthBonus;
			partHealth.HitPoints.UpdateValue();
		}
		EventBus.RaiseEvent(delegate(IUpgradeSystemComponentHandler h)
		{
			h.HandleSystemComponentDowngrade(SystemComponentType.InternalStructure, result);
		});
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
