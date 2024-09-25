using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("d6b0a46b654c71945a3a53f1e98347a4")]
public class HasScrap : Condition
{
	[SerializeField]
	private int scrapRequired;

	protected override string GetConditionCaption()
	{
		return $"Player has at least {scrapRequired} scrap";
	}

	protected override bool CheckCondition()
	{
		return (int)Game.Instance.Player.Scrap >= scrapRequired;
	}
}
