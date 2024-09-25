using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4515e962e6b6e6e43a2206ad6fcd0caa")]
public class CheckIsSpaceCombatGetter : MechanicEntityPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!Game.Instance.IsSpaceCombat)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "State is SpaceCombat";
	}
}
