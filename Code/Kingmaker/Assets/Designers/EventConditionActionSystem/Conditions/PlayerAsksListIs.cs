using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;

[TypeId("43679488caa342e6999125ae0f39c343")]
public class PlayerAsksListIs : Condition
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitAsksListReference m_Asks;

	private BlueprintUnitAsksList Asks => m_Asks?.Get();

	protected override string GetConditionCaption()
	{
		return $"Player asks list is ({Asks})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.MainCharacterEntity?.Asks?.List == Asks;
	}
}
