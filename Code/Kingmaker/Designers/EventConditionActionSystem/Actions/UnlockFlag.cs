using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/UnlockFlag")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("4a2be3982ea87e44f8d704b9a6330f57")]
public class UnlockFlag : GameAction, IUnlockableFlagReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("flag")]
	private BlueprintUnlockableFlagReference m_flag;

	public int flagValue;

	public BlueprintUnlockableFlag flag
	{
		get
		{
			return m_flag?.Get();
		}
		set
		{
			m_flag = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintUnlockableFlagReference>();
		}
	}

	public override void RunAction()
	{
		if (!flag.IsUnlocked)
		{
			flag.Unlock();
		}
		flag.Value = flagValue;
	}

	public override string GetCaption()
	{
		return $"Unlock Flag ({flag} to {flagValue})";
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag f)
	{
		if (flag == f)
		{
			return ((flagValue != 0) ? UnlockableFlagReferenceType.SetValue : UnlockableFlagReferenceType.None) | UnlockableFlagReferenceType.Unlock;
		}
		return UnlockableFlagReferenceType.None;
	}
}
