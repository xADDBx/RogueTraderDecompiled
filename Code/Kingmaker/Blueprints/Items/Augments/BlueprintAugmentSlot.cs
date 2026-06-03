using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UI.Common;

namespace Kingmaker.Blueprints.Items.Augments;

[TypeId("97c7a0dfdcae4a6a8b1ef33a2918f5f0")]
public class BlueprintAugmentSlot : BlueprintScriptableObject
{
	public ItemsFilterType AugmentFilterType;

	public bool IsMechSlot;

	[KDB("Аугмент, который будет устанавливаться в слот взамен потерянных конечностей. Не путать с аугментами,которые доступны на персонажах изначально - они задаются через BlueprintUnit")]
	public BlueprintItemAugmentReference DefaultAugment;
}
