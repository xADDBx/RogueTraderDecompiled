using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d5debd59683c7064fa9393bd52c9a624")]
public class ContextActionPush : ContextAction
{
	private const int MaxAnimatedCells = 5;

	private const float CoincidentPositionThresholdSqr = 0.0001f;

	[InfoBox("Max Range is 5")]
	public ContextValue Cells;

	public bool ProvokeAttackOfOpportunity;

	[SerializeField]
	private bool m_UseFactOwnerAsCaster;

	[SerializeField]
	private bool m_PushBack;

	[SerializeField]
	[InfoBox("Push the target sideways (perpendicular to the caster's facing). Side is chosen randomly per push.")]
	private bool m_PushPerpendicular;

	public override string GetCaption()
	{
		return "Push" + (ProvokeAttackOfOpportunity ? " (provoke AoO)" : "");
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Target unit is missing");
			return;
		}
		if (base.Context.MaybeCaster == null || (m_UseFactOwnerAsCaster && base.Context.MaybeOwner == null))
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		MechanicEntity caster = (m_UseFactOwnerAsCaster ? base.Context.MaybeOwner : base.Context.MaybeCaster);
		Game.Instance.AbilityExecutor.Abilities.FirstItem((AbilityExecutionProcess process) => process.Context.MaybeCaster == base.TargetEntity)?.Detach();
		Vector3 fromPoint = GetFromPoint(base.TargetEntity);
		int distance = Math.Min(Cells.Calculate(base.Context), 5);
		EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
		{
			h.HandleUnitResultPush(distance, caster, base.Target.Entity, fromPoint);
		});
	}

	private Vector3 GetFromPoint(MechanicEntity target)
	{
		if (base.Projectile != null && (base.AbilityContext?.Ability?.Blueprint.IsGrenade).GetValueOrDefault())
		{
			return base.Projectile.GetTargetPoint();
		}
		if (m_PushBack)
		{
			PartMovable optional = target.GetOptional<PartMovable>();
			if (optional != null && optional.HasMotionThisSimulationTick)
			{
				return target.Position + (target.Position - optional.PreviousPosition).normalized;
			}
		}
		MechanicEntity mechanicEntity = (m_UseFactOwnerAsCaster ? base.Context.MaybeOwner : base.Caster);
		if (m_PushPerpendicular && mechanicEntity is AbstractUnitEntity abstractUnitEntity)
		{
			Vector3 vector = ((abstractUnitEntity.Random.Range(0, 2) == 0) ? mechanicEntity.Right : (-mechanicEntity.Right));
			return target.Position - vector;
		}
		Vector3 vector2 = mechanicEntity.Position;
		if ((target.Position - vector2).sqrMagnitude < 0.0001f)
		{
			vector2 = target.Position - mechanicEntity.Forward;
		}
		return vector2;
	}
}
