using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e20d2ab26cb2403fb4971555bc91bc4c")]
public class ContextActionRequestEndTurn : ContextAction
{
	public enum ContextEntityType
	{
		Caster,
		Owner,
		Target,
		Anyone
	}

	[SerializeField]
	private ContextEntityType m_ContextEntity;

	private MechanicEntity TurnEntity => m_ContextEntity switch
	{
		ContextEntityType.Caster => base.Context.MaybeCaster, 
		ContextEntityType.Owner => base.Context.MaybeOwner, 
		ContextEntityType.Target => base.Target.Entity, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public override string GetCaption()
	{
		return $"Request end turn for {m_ContextEntity}";
	}

	protected override void RunAction()
	{
		if (m_ContextEntity == ContextEntityType.Anyone || Game.Instance.TurnController.CurrentUnit == TurnEntity)
		{
			Game.Instance.TurnController.RequestEndTurn();
		}
	}
}
