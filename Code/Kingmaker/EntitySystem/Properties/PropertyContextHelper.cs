using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyContextHelper
{
	[CanBeNull]
	public static MechanicEntity GetTargetEntity(this PropertyContext context, PropertyTargetType type, AbstractUnitEvaluator entityEvaluator = null)
	{
		switch (type)
		{
		case PropertyTargetType.CurrentEntity:
			return context.CurrentEntity;
		case PropertyTargetType.CurrentTarget:
			return context.CurrentTarget;
		case PropertyTargetType.ContextCaster:
			return context.ContextCaster;
		case PropertyTargetType.ContextMainTarget:
			return context.ContextMainTarget;
		case PropertyTargetType.RuleInitiator:
			return context.RuleInitiator;
		case PropertyTargetType.RuleTarget:
			return context.RuleTarget;
		case PropertyTargetType.EvaluatedTarget:
			using (ContextData<PropertyContextData>.Request().Setup(context.WithCurrentEntity(context.CurrentEntity)))
			{
				return context.GetEvaluatedTarget(entityEvaluator);
			}
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	[CanBeNull]
	public static MechanicEntity GetEvaluatedTarget(this PropertyContext context, AbstractUnitEvaluator entityEvaluator = null)
	{
		return entityEvaluator?.GetValue() ?? context.CurrentEntity;
	}

	[CanBeNull]
	public static Vector3? GetTargetPosition(this PropertyContext context, PropertyTargetType type, AbstractUnitEvaluator entityEvaluator = null)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => context.CurrentEntity.Position, 
			PropertyTargetType.CurrentTarget => context.CurrentTargetPosition, 
			PropertyTargetType.ContextCaster => context.ContextCaster?.Position, 
			PropertyTargetType.ContextMainTarget => context.ContextMainTargetPosition, 
			PropertyTargetType.RuleInitiator => context.RuleInitiator?.Position, 
			PropertyTargetType.RuleTarget => context.RuleTarget?.Position, 
			PropertyTargetType.EvaluatedTarget => context.GetEvaluatedTarget(entityEvaluator)?.Position, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	[CanBeNull]
	public static IntRect? GetTargetRectByType(this PropertyContext context, PropertyTargetType type)
	{
		BaseUnitEntity obj = context.GetTargetEntity(type) as BaseUnitEntity;
		if (obj == null)
		{
			if (!context.GetTargetPosition(type).HasValue)
			{
				return null;
			}
			return default(IntRect);
		}
		return obj.SizeRect;
	}

	public static string ToShortString(this PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => "CE", 
			PropertyTargetType.CurrentTarget => "CT", 
			PropertyTargetType.ContextCaster => "CC", 
			PropertyTargetType.ContextMainTarget => "CMT", 
			PropertyTargetType.RuleInitiator => "RI", 
			PropertyTargetType.RuleTarget => "RT", 
			PropertyTargetType.EvaluatedTarget => "ET", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static string Colorized(this PropertyTargetType type)
	{
		if (type == PropertyTargetType.CurrentEntity)
		{
			type = FormulaTargetScope.CurrentTarget;
		}
		if (FormulaTargetScope.NeedColorization)
		{
			return $"<color='green'>{type}</color>";
		}
		return type.ToString();
	}
}
