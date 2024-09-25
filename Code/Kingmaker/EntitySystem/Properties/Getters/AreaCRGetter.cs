using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3896c7c92dfd48cab40ad6ae5dff83d5")]
public class AreaCRGetter : PropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Area CR";
	}

	protected override int GetBaseValue()
	{
		return Game.Instance.LoadedAreaState.Blueprint.GetCR();
	}
}
