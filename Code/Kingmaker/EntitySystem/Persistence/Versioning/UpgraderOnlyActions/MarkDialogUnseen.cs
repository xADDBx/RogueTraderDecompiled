using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("9c73ac5115a249b3a5536205fd07c985")]
public class MarkDialogUnseen : PlayerUpgraderOnlyAction
{
	public BlueprintDialogReference Dialog;

	protected override void RunActionOverride()
	{
		BlueprintDialog blueprintDialog = Dialog.Get();
		if ((bool)blueprintDialog)
		{
			Game.Instance.Player.Dialog.ShownDialogsRemove(blueprintDialog);
		}
	}

	public override string GetCaption()
	{
		return $"Mark dialog {Dialog} unseen";
	}

	public override string GetDescription()
	{
		return $"Mark dialog {Dialog} unseen";
	}
}
