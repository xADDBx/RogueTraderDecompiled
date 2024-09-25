using System;
using Kingmaker.Blueprints.Items;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class MultikeyRestrictionSettings : NeedItemRestrictionSettings
{
	public override BlueprintItem GetItem()
	{
		return Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MultikeyItem;
	}
}
