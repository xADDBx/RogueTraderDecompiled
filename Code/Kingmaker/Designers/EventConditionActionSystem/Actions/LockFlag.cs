using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/LockFlag")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("0e571e9f07a314048afedff605fa53ce")]
public class LockFlag : GameAction, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	public override string GetDescription()
	{
		return $"Лочит флаг {Flag}";
	}

	public override void RunAction()
	{
		Flag.Lock();
	}

	public override string GetCaption()
	{
		return $"Lock Flag ({Flag})";
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag f)
	{
		if (Flag == f)
		{
			return UnlockableFlagReferenceType.Lock;
		}
		return UnlockableFlagReferenceType.None;
	}
}
