using System;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Components;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("37edf2f11f254c7797131ed55af32d50")]
public class DOTValueGetter : MechanicEntityPropertyGetter
{
	public DOT Type;

	[Tooltip("Calculated damage takes into consideration penetration and possible effects of other buffs and features. By default we simply take basic damage of DOT without any additional calculations.")]
	public bool UseCalculatedDamage;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"DoT value of {Type}";
	}

	protected override int GetBaseValue()
	{
		if (!UseCalculatedDamage)
		{
			return DOTLogic.GetBasicDamageOfType(base.CurrentEntity, Type);
		}
		return DOTLogic.GetCurrentDamageOfType(base.CurrentEntity, Type);
	}
}
