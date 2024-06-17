using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("12aaa30847dd4bab9ada22601914e152")]
public class MechanicFeatureGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private MechanicsFeatureType m_FeatureType;

	[SerializeField]
	private PropertyTargetType m_Target;

	protected override int GetBaseValue()
	{
		if (((this.GetTargetByType(m_Target) as BaseUnitEntity) ?? throw new Exception($"MechanicFeatureGetter: can't find suitable target of type {m_Target}")).GetMechanicFeature(m_FeatureType).Count <= 0)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return $"Has unit {m_FeatureType} feature";
	}
}
