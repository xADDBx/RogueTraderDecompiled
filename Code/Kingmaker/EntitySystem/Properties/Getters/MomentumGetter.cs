using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("613eb7155e8e4e92975d85768eec0f70")]
public class MomentumGetter : PropertyGetter
{
	protected override int GetBaseValue()
	{
		return (Game.Instance.TurnController.MomentumController.Groups.FindOrDefault((MomentumGroup p) => p.Units.Contains(GameHelper.GetPlayerCharacter())) ?? throw new Exception("MomentumGetter: couldn't find Player's momentumGroup. Check if Player is in combat")).Momentum;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Player party's Momentum";
	}
}
