using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d5debd59683c7064fa9393bd52c9a624")]
public class ContextActionPush : ContextAction
{
	private const int MaxAnimatedCells = 5;

	[InfoBox("Max Range is 5")]
	public ContextValue Cells;

	public bool ProvokeAttackOfOpportunity;

	[SerializeField]
	private bool m_UseFactOwnerAsCaster;

	public override string GetCaption()
	{
		return "Push" + (ProvokeAttackOfOpportunity ? " (provoke AoO)" : "");
	}

	public override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return;
		}
		if (base.Context.MaybeCaster == null || (m_UseFactOwnerAsCaster && base.Context.MaybeOwner == null))
		{
			PFLog.Default.Error("Caster is missing");
			return;
		}
		MechanicEntity caster = (m_UseFactOwnerAsCaster ? base.Context.MaybeOwner : base.Context.MaybeCaster);
		Vector3 fromPoint = GetFromPoint();
		int distance = Math.Min(Cells.Calculate(base.Context), 5);
		EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
		{
			h.HandleUnitResultPush(distance, caster, base.Target.Entity, fromPoint);
		});
	}

	private Vector3 GetFromPoint()
	{
		if (base.Projectile != null && (base.AbilityContext?.Ability?.Blueprint.IsGrenade).GetValueOrDefault())
		{
			return base.Projectile.GetTargetPoint();
		}
		if (!m_UseFactOwnerAsCaster)
		{
			return base.Caster.Position;
		}
		return base.Context.MaybeOwner.Position;
	}
}
