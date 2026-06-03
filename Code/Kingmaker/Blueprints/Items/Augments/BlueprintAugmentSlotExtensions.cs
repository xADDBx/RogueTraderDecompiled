using System.Linq;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.Blueprints.Items.Augments;

public static class BlueprintAugmentSlotExtensions
{
	public static bool IsCommon(this BlueprintAugmentSlot slot)
	{
		return (SimpleBlueprintExtendAsObject.Or(BlueprintRoot.Instance.Augments, null)?.CommonSlots?.Contains(slot)).GetValueOrDefault();
	}
}
