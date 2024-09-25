using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/FlagValue")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("d544653997f745c4ebead9b1719c7830")]
public class FlagValue : IntEvaluator, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	protected override int GetValueInternal()
	{
		return Flag.Value;
	}

	public override string GetCaption()
	{
		return $"Flag {Flag} value";
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (Flag == flag)
		{
			return UnlockableFlagReferenceType.CheckValue;
		}
		return UnlockableFlagReferenceType.None;
	}
}
