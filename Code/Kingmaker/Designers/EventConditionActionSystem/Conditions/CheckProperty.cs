using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("476772c7487f42bf8f9e0cd3edd9854c")]
public class CheckProperty : Condition
{
	private enum TargetType
	{
		None,
		Entity,
		Point
	}

	public PropertyCalculator Value;

	public bool SetCaster;

	[SerializeReference]
	public MechanicEntityEvaluator Caster;

	[SerializeField]
	private TargetType m_SetTarget;

	[CanBeNull]
	[SerializeReference]
	public MechanicEntityEvaluator TargetEntityEvaluator;

	[CanBeNull]
	[SerializeReference]
	public MechanicEntityEvaluator TargetPositionEvaluator;

	protected override string GetConditionCaption()
	{
		return $"Check property: {Value}";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity caster = GetCaster();
		TargetWrapper target = GetTarget();
		MechanicsContext maybeContext = caster.MainFact.MaybeContext;
		using (maybeContext.GetDataScope())
		{
			return Value.GetBoolValue(new PropertyContext(caster, null, target?.Entity, maybeContext));
		}
	}

	[NotNull]
	private MechanicEntity GetCaster()
	{
		if (!SetCaster)
		{
			return SimpleCaster.GetFree();
		}
		return Caster.GetValue();
	}

	[CanBeNull]
	private TargetWrapper GetTarget()
	{
		return m_SetTarget switch
		{
			TargetType.None => null, 
			TargetType.Entity => TargetEntityEvaluator.GetValue(), 
			TargetType.Point => TargetPositionEvaluator.GetValue(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
