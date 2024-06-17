using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.SystemMap;
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
		if (ContextData<StarSystemContextData>.Current?.StarSystemObject is AnomalyEntityData anomalyEntityData)
		{
			DialogData dialogData = DialogController.SetupDialogWithoutTarget(Dialog, null);
			dialogData.AddContextData<StarSystemContextData>().Setup(anomalyEntityData, null, null, anomalyEntityData.OnInteractionEnded);
			Game.Instance.DialogController.StartDialog(dialogData);
		}
	}
}
