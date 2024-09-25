using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("39d48af336aa4676a18d4565af334343")]
public class NotMainTargetGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (!(this.GetRule()?.Reason.Context?.MainTarget != base.CurrentEntity))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "1 if " + FormulaTargetScope.Current + " is not main target";
	}
}
