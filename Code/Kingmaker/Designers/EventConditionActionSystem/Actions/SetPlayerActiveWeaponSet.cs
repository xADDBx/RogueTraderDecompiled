using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("aa7344daa4944044aa1d74b62e3c246c")]
public class SetPlayerActiveWeaponSet : GameAction
{
	[SerializeField]
	[Tooltip("Need select custom unit evaluator.")]
	private bool m_SelectTargetUnit;

	[ShowIf("m_SelectTargetUnit")]
	[ValidateNotNull]
	[SerializeReference]
	[SerializeField]
	[Tooltip("Target unit evaluator.")]
	private AbstractUnitEvaluator m_TargetUnit;

	[SerializeField]
	[Tooltip("Select weapon set number for a weapon. Valid index 1 or 2!")]
	private int m_WeaponSet;

	public override string GetCaption()
	{
		string arg = "player";
		if (m_SelectTargetUnit)
		{
			arg = m_TargetUnit?.name ?? "unknown";
		}
		return $"Switch {arg} weapon set to {m_WeaponSet}";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity;
		if (m_SelectTargetUnit)
		{
			baseUnitEntity = m_TargetUnit.GetValue() as BaseUnitEntity;
			if (baseUnitEntity == null)
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {m_TargetUnit} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
		}
		else
		{
			baseUnitEntity = GameHelper.GetPlayerCharacter();
		}
		PartUnitBody partUnitBody = baseUnitEntity?.Body;
		int num = m_WeaponSet - 1;
		if (partUnitBody != null && partUnitBody.CurrentHandEquipmentSetIndex != num)
		{
			partUnitBody.CurrentHandEquipmentSetIndex = num;
		}
	}
}
