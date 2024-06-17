using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("718e0c48c4a64be8a64a127f85f40511")]
public class VailRankGetter : PropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "Vail value (number)";
	}

	protected override int GetBaseValue()
	{
		return Game.Instance.TurnController.VeilThicknessCounter.Value;
	}
}
