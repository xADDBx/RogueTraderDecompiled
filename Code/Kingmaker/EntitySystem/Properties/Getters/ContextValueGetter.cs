using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("71959818449d4891939b89c19a6d9a91")]
public class ContextValueGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public ContextValue Value;

	protected override string GetInnerCaption()
	{
		return Value.ToString();
	}

	protected override int GetBaseValue()
	{
		MechanicsContext mechanicsContext = this.GetMechanicContext();
		if (Value.IsValueSimple)
		{
			return Value.Calculate(mechanicsContext);
		}
		if (mechanicsContext == null)
		{
			mechanicsContext = base.CurrentEntity.MainFact.MaybeContext;
		}
		if (mechanicsContext == null)
		{
			throw new Exception("Context is missing");
		}
		return Value.Calculate(mechanicsContext);
	}
}
