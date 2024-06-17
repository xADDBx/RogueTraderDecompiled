using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("fd03f97ef3d1416d9a95f874454886d9")]
public class SimplePropertyGetter : PropertyGetter
{
	public EntityProperty Property;

	protected override string GetInnerCaption()
	{
		return $"${Property}";
	}

	protected override int GetBaseValue()
	{
		return Property.GetValue(base.CurrentEntity);
	}
}
