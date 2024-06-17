using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("0755d2f5495e46639699729ec4a8956e")]
public class CurrentAreaPartIs : Condition
{
	[SerializeField]
	private BlueprintAreaPartReference m_AreaPart;

	public BlueprintAreaPart AreaPart => m_AreaPart?.Get();

	protected override string GetConditionCaption()
	{
		return "Current area part is " + AreaPart.NameSafe();
	}

	protected override bool CheckCondition()
	{
		return (Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.Player.SavedInAreaPart) == AreaPart;
	}
}
