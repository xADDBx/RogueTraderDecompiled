using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/RaiseDead")]
[PlayerUpgraderAllowed(false)]
[TypeId("66dfc08af22a32e4d8d89b367175aee3")]
public class RaiseDead : GameAction
{
	[SerializeField]
	[FormerlySerializedAs("companion")]
	private BlueprintUnitReference m_companion;

	public bool riseAllCompanions;

	public BlueprintUnit companion => m_companion?.Get();

	public override string GetCaption()
	{
		if (!riseAllCompanions)
		{
			return $"Raise ({companion})";
		}
		return "Raise Dead Companions";
	}

	protected override void RunAction()
	{
		if (riseAllCompanions)
		{
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (partyAndPet.LifeState.IsDead)
				{
					DoRise(partyAndPet);
				}
			}
			return;
		}
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (allCharacter.Blueprint == companion && allCharacter.LifeState.IsDead)
			{
				DoRise(allCharacter);
				break;
			}
		}
	}

	private static void DoRise([CanBeNull] BaseUnitEntity unit)
	{
		unit?.LifeState.Resurrect();
	}
}
