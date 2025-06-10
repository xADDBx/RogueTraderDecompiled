using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("6c85b6adaa18425d9c1c3cb5ef9f2ebf")]
public class UnitHasMeleeWeaponEquipped : Condition
{
	public enum HandSlotTypes
	{
		FirstHand,
		SecondHand,
		AllHands
	}

	public enum HandSetTypes
	{
		CurrentHandsSet,
		OtherHandsSet,
		AllHandsSet
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[Tooltip("В какой руке ищем")]
	private HandSlotTypes m_HandSlotType = HandSlotTypes.AllHands;

	[SerializeField]
	[Tooltip("В каком випон сете ищем")]
	private HandSetTypes m_HandSetType = HandSetTypes.AllHandsSet;

	protected override bool CheckCondition()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		if (m_HandSetType == HandSetTypes.CurrentHandsSet)
		{
			return HasMeleeWeaponInHandsEquipmentSet(baseUnitEntity.Body.CurrentHandsEquipmentSet);
		}
		foreach (HandsEquipmentSet handsEquipmentSet in baseUnitEntity.Body.HandsEquipmentSets)
		{
			if ((m_HandSetType == HandSetTypes.AllHandsSet || handsEquipmentSet != baseUnitEntity.Body.CurrentHandsEquipmentSet) && HasMeleeWeaponInHandsEquipmentSet(handsEquipmentSet))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasMeleeWeaponInHandsEquipmentSet(HandsEquipmentSet handsEquipmentSet)
	{
		if (handsEquipmentSet != null)
		{
			switch (m_HandSlotType)
			{
			case HandSlotTypes.AllHands:
				if (!handsEquipmentSet.PrimaryHand.IsMelee)
				{
					return handsEquipmentSet.SecondaryHand.IsMelee;
				}
				return true;
			case HandSlotTypes.SecondHand:
				return handsEquipmentSet.SecondaryHand.IsMelee;
			case HandSlotTypes.FirstHand:
				return handsEquipmentSet.PrimaryHand.IsMelee;
			}
		}
		return false;
	}

	protected override string GetConditionCaption()
	{
		string text = "Выдает true";
		switch (m_HandSetType)
		{
		case HandSetTypes.CurrentHandsSet:
			text += " если в текущем випон сете";
			break;
		case HandSetTypes.OtherHandsSet:
			text += " если в остальных випон сетах, кроме текущего,";
			break;
		case HandSetTypes.AllHandsSet:
			text += " если во всех випон сетах";
			break;
		}
		switch (m_HandSlotType)
		{
		case HandSlotTypes.FirstHand:
			text += " есть мили оружие в главной руке.";
			break;
		case HandSlotTypes.SecondHand:
			text += " есть мили оружие во второй руке.";
			break;
		case HandSlotTypes.AllHands:
			text += " есть мили оружие в любой из рук.";
			break;
		}
		return text;
	}
}
