using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.Common;

namespace Kingmaker.Blueprints.Items.Augments;

[TypeId("97c7a0dfdcae4a6a8b1ef33a2918f5f0")]
public class BlueprintAugmentSlot : BlueprintScriptableObject
{
	public ItemsFilterType AugmentFilterType;

	public bool IsMechSlot;

	[KDB("Аугмент, который будет устанавливаться в слот взамен потерянных конечностей. Не путать с аугментами,которые доступны на персонажах изначально - они задаются через BlueprintUnit")]
	public BlueprintItemAugmentReference DefaultAugment;

	[KDB("Юниты, для которых при аугментации этого слота визуал НЕ нужно обновлять. Проверяется методом ShouldSkipVisualUpdate (используется из UI и тех-арта).")]
	public List<BlueprintUnitReference> SkipVisualUpdateUnits = new List<BlueprintUnitReference>();

	public bool ShouldSkipVisualUpdate(BlueprintUnit unit)
	{
		if (unit != null && SkipVisualUpdateUnits != null)
		{
			return SkipVisualUpdateUnits.HasReference(unit);
		}
		return false;
	}

	public bool ShouldSkipVisualUpdate(AbstractUnitEntity unit)
	{
		if (unit != null)
		{
			if (!ShouldSkipVisualUpdate(unit.Blueprint))
			{
				return ShouldSkipVisualUpdate(unit.OriginalBlueprint);
			}
			return true;
		}
		return false;
	}
}
