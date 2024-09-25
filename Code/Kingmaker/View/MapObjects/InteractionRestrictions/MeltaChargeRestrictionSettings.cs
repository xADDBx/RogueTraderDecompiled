using System;
using Kingmaker.Blueprints.Items;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class MeltaChargeRestrictionSettings : NeedItemRestrictionSettings
{
	public override BlueprintItem GetItem()
	{
		return Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem;
	}
}
