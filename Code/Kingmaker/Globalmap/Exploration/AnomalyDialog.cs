using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[TypeId("b243b88abf7c442981843c74cfbdbe21")]
public class AnomalyDialog : AnomalyInteraction
{
	[SerializeField]
	private BlueprintDialogReference m_Dialog;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	public override void Interact()
	{
		if (Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject is AnomalyEntityData)
		{
			DialogData data = DialogController.SetupDialogWithoutTarget(Dialog, null);
			Game.Instance.DialogController.StartDialog(data);
		}
	}
}
