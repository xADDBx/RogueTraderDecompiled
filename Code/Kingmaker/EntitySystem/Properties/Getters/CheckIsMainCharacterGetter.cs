using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("470ca8e024dd4c19a9f6d115fd554b04")]
public class CheckIsMainCharacterGetter : MechanicEntityPropertyGetter
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (Game.Instance.Player.MainCharacterEntity != base.CurrentEntity)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is Rogue Trader";
	}
}
