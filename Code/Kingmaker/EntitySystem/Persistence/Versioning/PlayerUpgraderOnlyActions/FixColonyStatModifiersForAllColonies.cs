using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("303b28e33e627074b85af6c6a4d05c26")]
public class FixColonyStatModifiersForAllColonies : PlayerUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Fix colony stat modifiers that are for all colonies: move Security/Efficiency entries from Contentment list to correct lists";
	}

	protected override void RunActionOverride()
	{
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		if (coloniesState.SecurityModifiersForAllColonies.Count > 0 || coloniesState.EfficiencyModifiersForAllColonies.Count > 0)
		{
			return;
		}
		List<ColonyStatModifier> list = new List<ColonyStatModifier>();
		List<ColonyStatModifier> list2 = new List<ColonyStatModifier>();
		foreach (ColonyStatModifier contentmentModifiersForAllColony in coloniesState.ContentmentModifiersForAllColonies)
		{
			if (IsSecurityModifier(contentmentModifiersForAllColony))
			{
				list.Add(contentmentModifiersForAllColony);
			}
			else if (IsEfficiencyModifier(contentmentModifiersForAllColony))
			{
				list2.Add(contentmentModifiersForAllColony);
			}
		}
		if (list.Count == 0 && list2.Count == 0)
		{
			return;
		}
		foreach (ColonyStatModifier item in list)
		{
			coloniesState.ContentmentModifiersForAllColonies.Remove(item);
			coloniesState.SecurityModifiersForAllColonies.Add(item);
		}
		foreach (ColonyStatModifier item2 in list2)
		{
			coloniesState.ContentmentModifiersForAllColonies.Remove(item2);
			coloniesState.EfficiencyModifiersForAllColonies.Add(item2);
		}
		foreach (ColoniesState.ColonyData colony in coloniesState.Colonies)
		{
			FixColony(colony.Colony, list, list2);
		}
	}

	private static void FixColony(Colony colony, List<ColonyStatModifier> securityModifiers, List<ColonyStatModifier> efficiencyModifiers)
	{
		foreach (ColonyStatModifier securityModifier in securityModifiers)
		{
			if (colony.Contentment.Modifiers.Remove(securityModifier))
			{
				colony.Security.Modifiers.Add(securityModifier);
			}
		}
		foreach (ColonyStatModifier efficiencyModifier in efficiencyModifiers)
		{
			if (colony.Contentment.Modifiers.Remove(efficiencyModifier))
			{
				colony.Efficiency.Modifiers.Add(efficiencyModifier);
			}
		}
	}

	private static bool IsSecurityModifier(ColonyStatModifier modifier)
	{
		if (modifier.Modifier == null)
		{
			return false;
		}
		foreach (RewardChangeStatSecurity item in modifier.Modifier.GetComponents<RewardChangeStatSecurity>().EmptyIfNull())
		{
			if (item.ApplyToAllColonies && item.SecurityModifier == (int)modifier.Value)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsEfficiencyModifier(ColonyStatModifier modifier)
	{
		if (modifier.Modifier == null)
		{
			return false;
		}
		foreach (RewardChangeStatEfficiency item in modifier.Modifier.GetComponents<RewardChangeStatEfficiency>().EmptyIfNull())
		{
			if (item.ApplyToAllColonies && item.EfficiencyModifier == (int)modifier.Value)
			{
				return true;
			}
		}
		return false;
	}
}
