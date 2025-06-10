using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("34f02a2ceadc2e442bdf2bfe54478571")]
public class DialogSeen : Condition
{
	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	protected override string GetConditionCaption()
	{
		return $"Dialog Seen ({Dialog})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.Dialog.ShownDialogsContains(Dialog);
	}
}
