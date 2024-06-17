using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic.Parts;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("fc71f02000fdf2a44aa5a50a2645a563")]
public class EquipmentRestrictionMachineTrait : EquipmentRestriction
{
	[InfoBox("Minimal rank of Part Machine Trait to equip an item")]
	public int MinRank = 1;

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		if (unit.Parts.GetOptional<PartMachineTrait>() == null)
		{
			return PartMachineTrait.GetBaseStatValue(unit) >= MinRank;
		}
		return (int)unit.Parts.GetOrCreate<PartMachineTrait>().MachineTrait >= MinRank;
	}
}
