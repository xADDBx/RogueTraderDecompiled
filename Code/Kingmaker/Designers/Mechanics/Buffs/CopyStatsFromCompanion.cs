using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[AllowMultipleComponents]
[TypeId("be95ae2d01be4649b6a2eb8a6447c740")]
public class CopyStatsFromCompanion : UnitBuffComponentDelegate, IHashable
{
	public bool FromOriginalMainCharacter;

	[HideIf("FromOriginalMainCharacter")]
	public BlueprintUnitReference Companion;

	public StatType Stat;

	protected override void OnActivateOrPostLoad()
	{
		BaseUnitEntity baseUnitEntity = (FromOriginalMainCharacter ? Game.Instance.Player.MainCharacterOriginal.ToBaseUnitEntity() : Game.Instance.Player.AllCharacters.Find((BaseUnitEntity u) => u.ToBaseUnitEntity().Blueprint == Companion.Get()));
		if (baseUnitEntity == null)
		{
			return;
		}
		int? num = baseUnitEntity.Stats.GetStat(Stat)?.ModifiedValue;
		if (!num.HasValue)
		{
			return;
		}
		ModifiableValue stat = base.Owner.Stats.GetStat(Stat);
		if (stat != null)
		{
			int num2 = num.Value - stat.ModifiedValue;
			if (num2 != 0)
			{
				stat.AddModifier(num2, base.Runtime, ModifierDescriptor.Polymorph);
			}
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat)?.RemoveModifiersFrom(base.Runtime);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
