using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("6db17b329e9084e4ab3fdce41be4ab99")]
public class CheckItemCondition : Condition
{
	private enum RequiredState
	{
		EquippedOn,
		NotEquippedOn
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_TargetItem;

	[SerializeField]
	private RequiredState m_RequiredState;

	[SerializeField]
	[InfoBox("If empty - check all units in party")]
	[SerializeReference]
	private AbstractUnitEvaluator m_UnitEvaluator;

	protected override string GetConditionCaption()
	{
		BlueprintItem blueprintItem = m_TargetItem.Get();
		if (blueprintItem == null)
		{
			return "[ERROR]: Items can't be null";
		}
		switch (m_RequiredState)
		{
		case RequiredState.EquippedOn:
			if (m_UnitEvaluator != null)
			{
				return $"Check {blueprintItem} is equipped for {m_UnitEvaluator}";
			}
			return $"Check {blueprintItem} is equipped on somebody in party";
		case RequiredState.NotEquippedOn:
			if (m_UnitEvaluator != null)
			{
				return $"Check {blueprintItem} is not equipped for ${m_UnitEvaluator}";
			}
			return $"Check {blueprintItem} is not equipped on anyone in party";
		default:
			return "[ERROR]: Not have valid caption";
		}
	}

	protected override bool CheckCondition()
	{
		BlueprintItem blueprintItem = m_TargetItem.Get();
		if (blueprintItem == null)
		{
			return false;
		}
		return m_RequiredState switch
		{
			RequiredState.EquippedOn => CheckEquippedOn(blueprintItem), 
			RequiredState.NotEquippedOn => CheckNotEquippedOn(blueprintItem), 
			_ => throw new NotImplementedException("Not implemented valid case: " + m_RequiredState), 
		};
	}

	private bool CheckNotEquippedOn(BlueprintItem item)
	{
		if (m_UnitEvaluator != null)
		{
			if (!(m_UnitEvaluator.GetValue() is BaseUnitEntity unit2))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {m_UnitEvaluator} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return false;
			}
			return !CheckOnUnit(unit2, item);
		}
		return Game.Instance.Player.Party.All((BaseUnitEntity unit) => !CheckOnUnit(unit, item));
	}

	private bool CheckEquippedOn(BlueprintItem item)
	{
		if (m_UnitEvaluator != null)
		{
			if (!(m_UnitEvaluator.GetValue() is BaseUnitEntity unit2))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {m_UnitEvaluator} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return false;
			}
			return CheckOnUnit(unit2, item);
		}
		return Game.Instance.Player.Party.Any((BaseUnitEntity unit) => CheckOnUnit(unit, item));
	}

	private static bool CheckOnUnit(BaseUnitEntity unit, BlueprintItem item)
	{
		ItemEntity result = null;
		unit?.Inventory.TryFind((ItemEntity x) => x.Blueprint.Equals(item), out result);
		if (result != null)
		{
			return result.Wielder?.Equals(unit) ?? false;
		}
		return false;
	}
}
