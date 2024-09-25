using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Interaction;

[ComponentName("Dialog/Start On Click")]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("80cbc40757a76fd41a5e5952e7c7bc3b")]
public class DialogOnClick : UnitInteractionComponent, IDialogReference
{
	[JsonProperty]
	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[JsonProperty]
	public ActionList NoDialogActions;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	[JsonConstructor]
	protected DialogOnClick()
	{
	}

	public override bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		if (!base.IsAvailable(initiator, target))
		{
			return false;
		}
		if (Dialog == null || Dialog.FirstCue.Cues.Count <= 0)
		{
			return NoDialogActions.HasActions;
		}
		return true;
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (!(target is BaseUnitEntity unit))
		{
			return AbstractUnitCommand.ResultType.Fail;
		}
		if (Dialog == null)
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				NoDialogActions.Run();
			}
			return AbstractUnitCommand.ResultType.Fail;
		}
		DialogData data = DialogController.SetupDialogWithUnit(Dialog, unit, user);
		Game.Instance.DialogController.StartDialog(data);
		return AbstractUnitCommand.ResultType.Success;
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
