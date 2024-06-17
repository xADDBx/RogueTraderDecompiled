using System;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("37edf2f11f254c7797131ed55af32d50")]
public class DOTValueGetter : MechanicEntityPropertyGetter
{
	public DOT Type;

	protected override string GetInnerCaption()
	{
		return $"DoT value of {Type}";
	}

	protected override int GetBaseValue()
	{
		return DOTLogic.GetCurrentDamageOfType(base.CurrentEntity, Type);
	}
}
