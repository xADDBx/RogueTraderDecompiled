using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[AllowedOn(typeof(BlueprintAbilityAreaEffectClusterLogic))]
[TypeId("2ca53113ae97252469a3609d4d73f686")]
public abstract class AbilityAreaEffectLogic : BlueprintComponent
{
	[Flags]
	private enum Options
	{
		None = 0,
		DoNotTreatEnterAsMovement = 1
	}

	[SerializeField]
	private Options m_Options;

	public void HandleUnitEnter(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		OnUnitEnter(context, areaEffect, unit);
		EventBus.RaiseEvent((IAreaEffectEntity)areaEffect, (Action<IAreaEffectEnterHandler>)delegate(IAreaEffectEnterHandler h)
		{
			h.HandleUnitEnterAreaEffect(unit);
		}, isCheckRuntime: true);
		if ((m_Options & Options.DoNotTreatEnterAsMovement) == 0)
		{
			HandleUnitMove(context, areaEffect, unit);
		}
	}

	public void HandleUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		OnUnitExit(context, areaEffect, unit);
	}

	public void HandleUnitMove(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		OnUnitMove(context, areaEffect, unit);
	}

	public void HandleRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnRound(context, areaEffect);
	}

	public void HandleTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnTick(context, areaEffect);
	}

	public void HandleEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnEndForEachUnit(context, areaEffect);
		OnEnd(context, areaEffect);
	}

	protected virtual void OnUnitEnter(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
	}

	protected virtual void OnUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
	}

	protected virtual void OnUnitMove(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
	}

	protected virtual void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEndForEachUnit(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}
}
