using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EtudeCompleteTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("b20b3c539fa60ed44abbfc7e367245ea")]
public class EtudeCompleteTrigger : EntityFactComponentDelegate, IEtudeCompleteTrigger, IHashable
{
	public ActionList Actions;

	private void OnCompleteInternal()
	{
		Actions.Run();
	}

	void IEtudeCompleteTrigger.OnComplete()
	{
		OnCompleteInternal();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
