using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Globalmap.Exploration;

[TypeId("6503da1a2ac74a5d8d621fc570d22b4a")]
public class AnomalyActivation : AnomalyInteraction
{
	public ActionList Actions;

	public override void Interact()
	{
		AnomalyEntityData obj = Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject as AnomalyEntityData;
		obj?.MainFact.RunActionInContext(Actions);
		obj?.OnInteractionEnded();
	}
}
