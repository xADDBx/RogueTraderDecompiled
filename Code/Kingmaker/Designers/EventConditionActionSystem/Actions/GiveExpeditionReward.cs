using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("4006e8b4ece547fa9684da5dd2e51bff")]
public class GiveExpeditionReward : GameAction
{
	private BlueprintStarSystemObjectReference m_StarSystemObject;

	[SerializeField]
	[Tooltip("Can be left as null, then check for any expedition from planet")]
	[CanBeNull]
	private BlueprintPointOfInterestReference m_PointOfInterest;

	public override string GetCaption()
	{
		return "Have expedition from " + m_StarSystemObject?.Get()?.Name;
	}

	public override void RunAction()
	{
		foreach (LootEntry item in (Enumerable.FirstOrDefault(Game.Instance.Player.StarSystemsState.SentExpedition, (ExpeditionInfo expedition) => expedition.StarSystemObject == m_StarSystemObject.Get() && (m_PointOfInterest?.Get() == null || expedition.PointOfInterest == m_PointOfInterest.Get()))?.Loot).EmptyIfNull())
		{
			GameHelper.GetPlayerCharacter().Inventory.Add(item.Item, item.Count);
		}
	}
}
