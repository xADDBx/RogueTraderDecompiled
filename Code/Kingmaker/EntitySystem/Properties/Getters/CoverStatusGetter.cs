using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.View.Covers;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("b9dd49398cf14b469d7fc78f01eb5e85")]
public class CoverStatusGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "CoverStatus";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		return baseUnitEntity.GetCoverType() switch
		{
			LosCalculations.CoverType.Full => 2, 
			LosCalculations.CoverType.Half => 1, 
			_ => 0, 
		};
	}
}
