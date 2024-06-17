using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValue
{
	[HideInInspector]
	public ContextValueType ValueType;

	[HideInInspector]
	[ShowIf("IsValueSimple")]
	public int Value;

	[HideInInspector]
	[ShowIf("IsValueRank")]
	public AbilityRankType ValueRank;

	[HideInInspector]
	[ShowIf("IsValueShared")]
	public AbilitySharedValue ValueShared;

	[HideInInspector]
	[ShowIf("IsValueProperty")]
	public EntityProperty Property;

	[SerializeField]
	[HideInInspector]
	[ShowIf("IsValueCustomProperty")]
	private BlueprintEntityPropertyReference m_CustomProperty;

	[SerializeField]
	[HideInInspector]
	[ShowIf("IsValueNamedProperty")]
	public ContextPropertyName PropertyName;

	public PropertyCalculatorBlueprint CustomProperty => m_CustomProperty?.Get();

	public bool IsZero
	{
		get
		{
			if (Value == 0)
			{
				return ValueType == ContextValueType.Simple;
			}
			return false;
		}
	}

	public bool IsValueSimple
	{
		get
		{
			if (ValueType != 0 && ValueType != ContextValueType.CasterBuffRank)
			{
				return ValueType == ContextValueType.TargetBuffRank;
			}
			return true;
		}
	}

	public bool IsValueRank => ValueType == ContextValueType.Rank;

	public bool IsValueShared => ValueType == ContextValueType.Shared;

	public bool IsValueProperty
	{
		get
		{
			if (ValueType != ContextValueType.CasterProperty)
			{
				return ValueType == ContextValueType.TargetProperty;
			}
			return true;
		}
	}

	public bool IsValueCustomProperty
	{
		get
		{
			if (ValueType != ContextValueType.CasterCustomProperty)
			{
				return ValueType == ContextValueType.TargetCustomProperty;
			}
			return true;
		}
	}

	public bool IsValueNamedProperty
	{
		get
		{
			if (ValueType != ContextValueType.CasterNamedProperty && ValueType != ContextValueType.TargetNamedProperty)
			{
				return ValueType == ContextValueType.ContextProperty;
			}
			return true;
		}
	}

	public int Calculate(MechanicsContext context)
	{
		if (!IsValueSimple && context == null)
		{
			PFLog.Default.Error("Context is missing");
			return 0;
		}
		BlueprintScriptableObject blueprint = context?.AssociatedBlueprint;
		switch (ValueType)
		{
		case ContextValueType.Simple:
			return Value;
		case ContextValueType.Rank:
			return 0;
		case ContextValueType.Shared:
			return context[ValueShared];
		case ContextValueType.CasterProperty:
			return Property.GetValue(context.MaybeCaster);
		case ContextValueType.TargetProperty:
		{
			MechanicEntity currentEntity = ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Entity ?? context.MainTarget.Entity;
			return Property.GetValue(currentEntity);
		}
		case ContextValueType.CasterCustomProperty:
			return CustomProperty.GetValue(ContextData<PropertyContextData>.Current?.Context ?? new PropertyContext(context.MaybeCaster, null, null, context));
		case ContextValueType.TargetCustomProperty:
		{
			MechanicEntity currentEntity = ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Entity ?? context.MainTarget.Entity;
			return CustomProperty.GetValue(ContextData<PropertyContextData>.Current?.Context ?? new PropertyContext(currentEntity, null, null, context));
		}
		case ContextValueType.CasterNamedProperty:
			return blueprint.GetPropertyValue(PropertyName, context.MaybeCaster, context);
		case ContextValueType.TargetNamedProperty:
		{
			MechanicEntity currentEntity = ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Entity ?? context.MainTarget.Entity;
			return blueprint.GetPropertyValue(PropertyName, currentEntity, context);
		}
		case ContextValueType.ContextProperty:
			return context[PropertyName];
		case ContextValueType.CasterBuffRank:
			if (!(context.AssociatedBlueprint is BlueprintBuff blueprint3))
			{
				return 0;
			}
			return (context.MaybeCaster?.Buffs?.Get(blueprint3)?.GetRank() * Value).GetValueOrDefault();
		case ContextValueType.TargetBuffRank:
			if (!(context.AssociatedBlueprint is BlueprintBuff blueprint2))
			{
				return 0;
			}
			return ((context.MaybeOwner ?? ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Entity ?? context.MainTarget.Entity)?.Buffs?.Get(blueprint2)?.GetRank() * Value).GetValueOrDefault();
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static implicit operator ContextValue(int value)
	{
		return new ContextValue
		{
			ValueType = ContextValueType.Simple,
			Value = value
		};
	}

	public override string ToString()
	{
		return ValueType switch
		{
			ContextValueType.Simple => Value.ToString(), 
			ContextValueType.Rank => $"Rnk.{ValueRank}", 
			ContextValueType.Shared => $"Shd.{ValueShared}", 
			ContextValueType.CasterProperty => $"C.{Property}", 
			ContextValueType.TargetProperty => $"T.{Property}", 
			ContextValueType.CasterCustomProperty => "C." + (SimpleBlueprintExtendAsObject.Or(CustomProperty, null)?.name ?? "<missing-property>"), 
			ContextValueType.TargetCustomProperty => "T." + (SimpleBlueprintExtendAsObject.Or(CustomProperty, null)?.name ?? "<missing-property>"), 
			ContextValueType.CasterNamedProperty => $"C.{PropertyName}", 
			ContextValueType.TargetNamedProperty => $"T.{PropertyName}", 
			ContextValueType.ContextProperty => $"Ctx.{PropertyName}", 
			ContextValueType.CasterBuffRank => $"This buff rank on caster * {Value}", 
			ContextValueType.TargetBuffRank => $"This buff rank on target * {Value}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
