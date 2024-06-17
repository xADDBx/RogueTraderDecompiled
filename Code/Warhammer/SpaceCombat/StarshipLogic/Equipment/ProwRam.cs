using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

public class ProwRam : SystemComponent, IHashable
{
	public int BonusDamage => base.Blueprint.ProwRamDamageBonus * base.UpgradeLevel;

	public int SelfDamageDeduction => base.Blueprint.ProwRamSelfDamageReduceBonus * base.UpgradeLevel;

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

	public ProwRam(StarshipEntity ship)
		: base(ship)
	{
	}

	public ProwRam(JsonConstructorMark _)
		: base(_)
	{
	}

	public override UpgradeResult Upgrade()
	{
		UpgradeResult result = UpgradeResult.Successful;
		if (base.Blueprint.UpgradeCost.Length <= base.UpgradeLevel + 1)
		{
			result = UpgradeResult.MaxUpgrade;
		}
		int num = base.Blueprint.UpgradeCost[base.UpgradeLevel + 1];
		if (num > (int)Game.Instance.Player.Scrap)
		{
			result = UpgradeResult.NotEnoughScrap;
		}
		if (result == UpgradeResult.Successful)
		{
			Game.Instance.Player.Scrap.Spend(num);
			base.UpgradeLevel++;
		}
		EventBus.RaiseEvent(delegate(IUpgradeSystemComponentHandler h)
		{
			h.HandleSystemComponentUpgrade(SystemComponentType.ProwRam, result);
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
		if (result == DowngradeResult.Successful)
		{
			base.UpgradeLevel--;
			int scrap = base.Blueprint.UpgradeCost[base.UpgradeLevel + 1];
			Game.Instance.Player.Scrap.Receive(scrap);
		}
		EventBus.RaiseEvent(delegate(IUpgradeSystemComponentHandler h)
		{
			h.HandleSystemComponentDowngrade(SystemComponentType.ProwRam, result);
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
