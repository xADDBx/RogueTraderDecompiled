using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CompanionIsDead")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("d00239901a4bf03408fc8b12116b4d67")]
public class CompanionIsDead : Condition
{
	[SerializeField]
	[FormerlySerializedAs("companion")]
	private BlueprintUnitReference m_companion;

	public bool anyCompanion;

	public BlueprintUnit companion => m_companion?.Get();

	protected override string GetConditionCaption()
	{
		if (!anyCompanion)
		{
			return $"Companion ({companion}) is dead";
		}
		return "Dead companion in party";
	}

	protected override bool CheckCondition()
	{
		if (anyCompanion)
		{
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (partyAndPet.LifeState.IsFinallyDead)
				{
					return true;
				}
			}
		}
		else
		{
			foreach (BaseUnitEntity partyAndPet2 in Game.Instance.Player.PartyAndPets)
			{
				if ((partyAndPet2.Blueprint == companion || partyAndPet2.Blueprint.PrototypeLink == companion) && partyAndPet2.LifeState.IsFinallyDead)
				{
					return true;
				}
			}
		}
		return false;
	}
}
