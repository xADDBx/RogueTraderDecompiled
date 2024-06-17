using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("8b570f3f321a21e4ba220a9d20cb6190")]
public class PsyRatingGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "PR";
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetPsykerOptional()?.PsyRating.ModifiedValue ?? 0;
	}
}
