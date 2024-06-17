using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;

[TypeId("21eaf44b8e5b8004f8ec912dc94b92bf")]
[PlayerUpgraderAllowed(false)]
public class AreaVisited : Condition
{
	[SerializeField]
	[FormerlySerializedAs("Area")]
	private BlueprintAreaReference m_Area;

	public BlueprintArea Area => m_Area?.Get();

	protected override string GetConditionCaption()
	{
		return $"Area Visited ({Area})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.VisitedAreas.Contains(Area);
	}
}
