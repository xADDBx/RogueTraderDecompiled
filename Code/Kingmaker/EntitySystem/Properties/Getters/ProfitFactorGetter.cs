using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b82814cf6da14e3ebe9776cc83effba0")]
public class ProfitFactorGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current Player's Profit Factor total value";
	}

	protected override int GetBaseValue()
	{
		return Mathf.FloorToInt(Game.Instance.Player.ProfitFactor.Total);
	}
}
