using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3821866a2e27cdb429f019da0833a621")]
public class FactionReputationLevelGetter : UnitPropertyGetter
{
	public FactionType Faction;

	protected override string GetInnerCaption()
	{
		return "Get Reputation level with chosen Faction";
	}

	protected override int GetBaseValue()
	{
		if (Faction != 0)
		{
			return ReputationHelper.GetCurrentReputationLevel(Faction);
		}
		return 0;
	}
}
