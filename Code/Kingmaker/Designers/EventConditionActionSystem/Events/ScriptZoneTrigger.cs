using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.SriptZones;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/ScriptZoneTrigger")]
[AllowMultipleComponents]
[TypeId("90d0afb327d532a46aca77ed64ae9bd6")]
public class ScriptZoneTrigger : EntityFactComponentDelegate, IScriptZoneHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	public string UnitRef;

	public ConditionsChecker OnEnterConditions;

	public ActionList OnEnterActions;

	public ConditionsChecker OnExitConditions;

	public ActionList OnExitActions;

	public void OnUnitEnteredScriptZone(ScriptZone zone)
	{
		if (!(zone == ScriptZone.FindView() as ScriptZone))
		{
			return;
		}
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity, zone.Data?.HoldingState))
		{
			if (OnEnterConditions.Check())
			{
				OnEnterActions.Run();
			}
		}
	}

	public void OnUnitExitedScriptZone(ScriptZone zone)
	{
		if (!(zone == ScriptZone.FindView() as ScriptZone))
		{
			return;
		}
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity, zone.Data?.HoldingState))
		{
			if (OnExitConditions.Check())
			{
				OnExitActions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
