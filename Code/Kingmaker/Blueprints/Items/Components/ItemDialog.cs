using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("724f41333a606fa4eb53cb430ac082cb")]
public class ItemDialog : BlueprintComponent
{
	[SerializeField]
	private LocalizedString m_ItemName;

	[SerializeField]
	private BlueprintDialogReference m_DialogReference;

	public BlueprintDialog Dialog => m_DialogReference?.Get();

	public LocalizedString ItemName => m_ItemName;

	public void StartDialog()
	{
		BlueprintDialog blueprintDialog = m_DialogReference.Get();
		if (blueprintDialog != null)
		{
			DialogData data = DialogController.SetupDialogWithoutTarget(blueprintDialog, m_ItemName, Game.Instance.Player.MainCharacterEntity);
			Game.Instance.DialogController.StartDialog(data);
		}
	}
}
