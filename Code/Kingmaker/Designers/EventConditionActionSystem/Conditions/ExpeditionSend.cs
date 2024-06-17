using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("c4737cb044be4448a075d120d4b70cdc")]
public class ExpeditionSend : Condition
{
	[SerializeField]
	private BlueprintStarSystemObjectReference m_StarSystemObject;

	[SerializeField]
	[Tooltip("Can be left as null, then check for any expedition from planet")]
	[CanBeNull]
	private BlueprintPointOfInterestReference m_PointOfInterest;

	protected override string GetConditionCaption()
	{
		return "Have expedition from " + m_StarSystemObject?.Get()?.Name;
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.StarSystemsState.SentExpedition.Any((ExpeditionInfo expedition) => expedition.StarSystemObject == m_StarSystemObject.Get() && (m_PointOfInterest?.Get() == null || expedition.PointOfInterest == m_PointOfInterest.Get()));
	}
}
