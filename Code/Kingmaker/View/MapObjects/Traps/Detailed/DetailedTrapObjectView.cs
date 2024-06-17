using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Detailed;

public class DetailedTrapObjectView : TrapObjectView, IBlueprintedMapObjectView
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintTrapReference m_Blueprint;

	public BlueprintTrap Blueprint => m_Blueprint?.Get();

	public static DetailedTrapObjectView CreateView(BlueprintTrap blueprint, string uniqueId, string scriptZoneId)
	{
		if (blueprint == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject(blueprint.name);
		gameObject.SetActive(value: false);
		DetailedTrapObjectView detailedTrapObjectView = gameObject.AddComponent<DetailedTrapObjectView>();
		ScriptZone scriptZoneTrigger = TrapObjectView.SetupViewAndFindScriptZone(detailedTrapObjectView, scriptZoneId, blueprint.DisarmAnimation);
		detailedTrapObjectView.UniqueId = uniqueId;
		detailedTrapObjectView.Settings = new TrapObjectViewSettings
		{
			ScriptZoneTrigger = scriptZoneTrigger
		};
		detailedTrapObjectView.ApplyBlueprint(blueprint);
		gameObject.SetActive(value: true);
		return detailedTrapObjectView;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Blueprint != null)
		{
			ApplyBlueprint(Blueprint);
		}
	}

	public override bool SupportBlueprint(BlueprintMapObject blueprint)
	{
		if (base.SupportBlueprint(blueprint))
		{
			return blueprint is BlueprintTrap;
		}
		return false;
	}

	public override void ApplyBlueprint(BlueprintMapObject blueprint)
	{
		base.ApplyBlueprint(blueprint);
		m_Blueprint = blueprint.ToReference<BlueprintTrapReference>();
		AwarenessCheckComponent awarenessCheckComponent = base.gameObject.EnsureComponent<AwarenessCheckComponent>();
		awarenessCheckComponent.DC = Blueprint.AwarenessDC;
		awarenessCheckComponent.Radius = Blueprint.AwarenessRadius;
	}

	protected override TrapObjectData CreateData()
	{
		return Entity.Initialize(new DetailedTrapObjectData(this));
	}
}
