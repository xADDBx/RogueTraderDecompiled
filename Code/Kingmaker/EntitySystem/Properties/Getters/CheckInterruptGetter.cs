using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5199ec18b22b8b1459f37128aebfda3d")]
public class CheckInterruptGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool CheckSourceOfInterrupt;

	[ShowIf("CheckSourceOfInterrupt")]
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity.Initiative.InterruptingOrder <= 0 && ContextData<InterruptTurnData>.Current?.Unit != base.CurrentEntity)
		{
			return 0;
		}
		if (CheckSourceOfInterrupt)
		{
			if (ContextData<InterruptTurnData>.Current?.Source != this.GetTargetByType(Target))
			{
				return 0;
			}
			return 1;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!CheckSourceOfInterrupt)
		{
			return "CurrentEntity's turn is an interrupt";
		}
		return $"CurrentEntity's turn is an interrupt from {Target}";
	}
}
