using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("740349afc5a347d088228fbba1ea243e")]
public class MarkDialogSeen : PlayerUpgraderOnlyAction
{
	public BlueprintDialogReference Dialog;

	protected override void RunActionOverride()
	{
		BlueprintDialog blueprintDialog = Dialog.Get();
		if ((bool)blueprintDialog)
		{
			Game.Instance.Player.Dialog.ShownDialogs.Add(blueprintDialog);
		}
	}

	public override string GetCaption()
	{
		return $"Mark dialog {Dialog} seen";
	}

	public override string GetDescription()
	{
		return $"Mark dialog {Dialog} seen";
	}
}
