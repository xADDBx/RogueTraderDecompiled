using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("806d343014d244b9ae4954eca79d2d10")]
public class AddColonyEvent : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public BlueprintPlanet Planet => m_Planet?.Get();

	public override string GetCaption()
	{
		return $"Add colony event {Event} to {Planet}";
	}

	protected override void RunAction()
	{
		ColoniesGenerator.AddEventToColony(Event, Planet);
	}
}
