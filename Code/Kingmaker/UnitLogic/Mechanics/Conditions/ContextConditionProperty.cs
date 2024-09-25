using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.QA;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("d271cb15605a459ca6188e939a2d5d72")]
public class ContextConditionProperty : ContextCondition
{
	public PropertyCalculator Property;

	public bool NegativeDoesNotCount;

	protected override string GetConditionCaption()
	{
		return Property.ToString();
	}

	protected override bool CheckCondition()
	{
		MechanicEntity mechanicEntity = base.Target.Entity ?? base.Context.MaybeCaster;
		if (mechanicEntity == null)
		{
			PFLog.Default.ErrorWithReport("CurrentEntity is missing");
			return false;
		}
		if (!NegativeDoesNotCount)
		{
			return Property.GetValue(new PropertyContext(mechanicEntity, null, null, base.Context)) != 0;
		}
		return Property.GetValue(new PropertyContext(mechanicEntity, null, null, base.Context)) > 0;
	}
}
