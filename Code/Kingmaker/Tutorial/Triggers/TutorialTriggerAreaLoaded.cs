using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("70a04c7e5135ef6478ba90578bc9d2d2")]
public class TutorialTriggerAreaLoaded : TutorialTrigger, IAreaActivationHandler, ISubscriber, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_Area;

	[SerializeField]
	[InfoBox("Always true if empty")]
	private ConditionsChecker m_Condition;

	public void OnAreaActivated()
	{
		if (Game.Instance.CurrentlyLoadedArea == m_Area.Get() && (!m_Condition.HasConditions || m_Condition.Check()))
		{
			TryToTrigger(null);
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
