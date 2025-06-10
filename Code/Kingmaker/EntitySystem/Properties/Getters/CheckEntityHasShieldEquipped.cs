using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7b30b9ae7a50470782a2e29bec4baaf3")]
public class CheckEntityHasShieldEquipped : PropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		PartUnitBody bodyOptional = base.CurrentEntity.GetBodyOptional();
		if (bodyOptional == null)
		{
			return 0;
		}
		if (!bodyOptional.Hands.Any((HandSlot p) => p.HasShield))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current entity has a shield in one of their hands";
	}
}
