using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CompanionInParty")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("d2f424beb5ace314887e9cc946b68dfa")]
public class CompanionInParty : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("companion")]
	private BlueprintUnitReference m_companion;

	[Tooltip("Зарекручен и находится в активной партии")]
	public bool MatchWhenActive = true;

	[Tooltip("Зарекручен, находится в партии, но не управляется игроком в данный момент (стоит на месте, где находился в момент детача, пропадает из панели партии)")]
	public bool MatchWhenDetached;

	[Tooltip("Зарекручен, но в данный момент не находится в активной партии")]
	public bool MatchWhenRemote;

	[Tooltip("Анрекручен, т.е. удален из ростера")]
	public bool MatchWhenEx;

	[Tooltip("Мертв")]
	public bool IncludeDead;

	public BlueprintUnit companion => m_companion?.Get();

	protected override string GetConditionCaption()
	{
		return $"Companion ({companion}) in party";
	}

	protected override bool CheckCondition()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (allCharacter.Blueprint == companion || allCharacter.Blueprint.PrototypeLink == companion)
			{
				CompanionState companionState = allCharacter.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				if ((MatchWhenActive || companionState != CompanionState.InParty) && (MatchWhenRemote || companionState != CompanionState.Remote) && (MatchWhenDetached || companionState != CompanionState.InPartyDetached) && (MatchWhenEx || companionState != CompanionState.ExCompanion) && (IncludeDead || !allCharacter.LifeState.IsFinallyDead))
				{
					return true;
				}
			}
		}
		return false;
	}
}
