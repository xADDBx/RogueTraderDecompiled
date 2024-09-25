using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("71d0da4945314ed19f7dc20cace0b7bb")]
public class AscensionDistanceToStartTurnSpotGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		UnitPartAscensionParameters optional = base.CurrentEntity.Parts.GetOptional<UnitPartAscensionParameters>();
		if (optional == null)
		{
			return 0;
		}
		return base.CurrentEntity.DistanceToInCells(optional.StartTurnPosition);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance from " + FormulaTargetScope.Current + " to starting position this turn";
	}
}
