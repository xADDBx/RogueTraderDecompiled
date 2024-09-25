using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueTemporaryHitPoints : ModifiableValue, IHashable
{
	protected override int CalculateBaseValue(int baseValue)
	{
		return 0;
	}

	public int HandleDamage(int damage)
	{
		List<Modifier> list = null;
		foreach (Modifier modifier in base.Modifiers)
		{
			int num = Math.Max(0, damage - modifier.ModValue);
			modifier.ModValue -= damage;
			if (modifier.ModValue < 1)
			{
				if (list == null)
				{
					list = TempList.Get<Modifier>();
				}
				list.Add(modifier);
			}
			damage = num;
			if (damage < 1)
			{
				break;
			}
		}
		list?.ForEach(delegate(Modifier m)
		{
			m.Remove();
		});
		return damage;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
