using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("6644a1aff0f24b4fac0fd20fcd9f26f3")]
public class CurrentAreaIs : Condition
{
	[SerializeField]
	[FormerlySerializedAs("Area")]
	private BlueprintAreaReference m_Area;

	public BlueprintArea Area => m_Area?.Get();

	protected override string GetConditionCaption()
	{
		return $"Current area is {Area.NameSafe()}";
	}

	protected override bool CheckCondition()
	{
		return (Game.Instance.CurrentlyLoadedArea ?? Game.Instance.Player.SavedInArea) == Area;
	}
}
