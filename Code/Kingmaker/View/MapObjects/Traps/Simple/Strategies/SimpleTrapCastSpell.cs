using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.View.MapObjects.Traps.Simple.Strategies;

public static class SimpleTrapCastSpell
{
	public static void Invoke(SimpleTrapObjectData data)
	{
		Ability ability;
		if (!data.Info.IsSpell)
		{
			if (!data.Info.IsWeapon)
			{
				throw new Exception("Can't find ability");
			}
			ability = data.RequestWeaponAbility(data.Info.BlueprintWeapon, data.Info.WeaponAbilityType);
		}
		else
		{
			ability = data.RequestAbility(data.Info.BlueprintSpell);
		}
		Ability ability2 = ability;
		List<TargetWrapper> list = SelectTargets(data);
		if (list.Count == 0)
		{
			PFLog.Default.Log($"No suitable targets for trap {data}. Will not cast spell.");
		}
		else
		{
			ApplyAbilityToTargets(data, ability2, list);
		}
	}

	private static List<TargetWrapper> SelectTargets(SimpleTrapObjectData data)
	{
		List<TargetWrapper> list = TempList.Get<TargetWrapper>();
		switch (data.Info.SpellAnchor)
		{
		case TrapSpellAnchor.Point:
			list.Add(data.Settings.TargetPoint.transform.position);
			break;
		case TrapSpellAnchor.Victim:
		{
			BaseUnitEntity baseUnitEntity = ContextData<BlueprintTrap.ElementsData>.Current?.TriggeringUnit;
			if (baseUnitEntity != null)
			{
				list.Add(baseUnitEntity);
			}
			break;
		}
		case TrapSpellAnchor.PartyLos:
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (LosCalculations.HasLos(data, partyAndPet))
				{
					list.Add(partyAndPet);
				}
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return list;
	}

	private static void ApplyAbilityToTargets(SimpleTrapObjectData data, Ability ability, IEnumerable<TargetWrapper> targets)
	{
		if (!targets.Any())
		{
			return;
		}
		BlueprintTrapSettings trapSettings = data.TrapSettings;
		float secondsBetweenAbilityActions = data.Info.SecondsBetweenAbilityActions;
		SimpleCaster free = SimpleCaster.GetFree();
		free.IsTrap = true;
		free.TrapParentObject = data.View.gameObject.transform.parent.gameObject;
		free.NameInLog = data.View.NameInLog;
		if (data.Settings.ActorPosition != null)
		{
			free.Position = data.Settings.ActorPosition.transform.position;
		}
		AbilityData spell = new AbilityData(ability, free)
		{
			OverrideCasterLevel = trapSettings.ActorLevel,
			OverrideCasterModifier = trapSettings.ActorStatMod.PickRandom()
		};
		foreach (TargetWrapper target in targets)
		{
			if (!(target == null))
			{
				Rulebook.Trigger(new RulePerformAbility(spell, target)).Context.RewindActionIndex(secondsBetweenAbilityActions.Seconds());
			}
		}
	}
}
