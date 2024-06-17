using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Interaction;

public class SpawnerInteractionDialog : SpawnerInteraction, IDialogReference
{
	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (!(target is BaseUnitEntity unit))
		{
			return AbstractUnitCommand.ResultType.Fail;
		}
		if (Dialog == null)
		{
			return AbstractUnitCommand.ResultType.Success;
		}
		DialogData data = DialogController.SetupDialogWithUnit(Dialog, unit, user);
		Game.Instance.DialogController.StartDialog(data);
		return AbstractUnitCommand.ResultType.Success;
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (Dialog != dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
