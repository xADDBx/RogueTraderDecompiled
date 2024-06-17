using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueAttributeStat : ModifiableValue, IHashable
{
	[JsonProperty]
	public readonly CountableFlag Disabled = new CountableFlag();

	public int Damage
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int Drain
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool HasPenalties => base.Modifiers.Any((Modifier m) => !m.IsPermanent() && m.ModValue < 0);

	public bool HasBonuses => base.Modifiers.Any((Modifier m) => !m.IsPermanent() && m.ModValue > 0);

	public bool Enabled => !Disabled;

	protected override int MinValue => 1;

	protected override bool IgnoreModifiers => Disabled;

	public int Bonus => base.ModifiedValue / 10;

	public int WarhammerBonus => base.ModifiedValue / 10;

	public int PermanentBonus => base.PermanentValue / 10;

	private void UpdateDamageStatusBuff(bool hadDamage, bool hasDamage, bool drain)
	{
		if (!(base.Owner is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		BlueprintBuff damageBuff = BlueprintRoot.Instance.StatusBuffs.GetDamageBuff(base.Type, drain);
		if (damageBuff != null)
		{
			bool num = baseUnitEntity.Facts.Contains(damageBuff);
			if (num && !hasDamage)
			{
				baseUnitEntity.Facts.Remove(damageBuff);
			}
			if (!num && hasDamage)
			{
				baseUnitEntity.Buffs.Add(damageBuff, baseUnitEntity);
			}
		}
	}

	protected override int CalculateBaseValue(int baseValue)
	{
		if (!Disabled)
		{
			return base.CalculateBaseValue(baseValue);
		}
		return 10;
	}

	public void RetainDisabled()
	{
		bool num = Disabled;
		Disabled.Retain();
		if (num != (bool)Disabled)
		{
			UpdateValue();
		}
	}

	public void ReleaseDisabled()
	{
		bool num = Disabled;
		Disabled.Release();
		if (num != (bool)Disabled)
		{
			UpdateValue();
		}
	}

	public void ValidateFreezedModifiers()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<CountableFlag>.GetHash128(Disabled);
		result.Append(ref val2);
		return result;
	}
}
