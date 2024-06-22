using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("bcd46c75387a6fc4592918b7a974475f")]
public class CurrentWeaponDamageTypeGetter : MechanicEntityPropertyGetter
{
	public DamageType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Current weapon damage == {Type}";
	}

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity.GetFirstWeapon()?.Blueprint.DamageType.Type != Type)
		{
			return 0;
		}
		return 1;
	}
}
