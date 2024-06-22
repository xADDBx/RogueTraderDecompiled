using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5dc48d0314f446d5b9716579d5edd2b1")]
public class IsEngagedGetter : MechanicEntityPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!base.CurrentEntity.IsEngagedInMelee())
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " engaged in melee combat.";
	}
}
