using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0fe139d89ec84e26a8e69291e53ed09a")]
public class MoveEventFromStartedToScheduled : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public override string GetCaption()
	{
		return $"Move event {Event} from started to scheduled";
	}

	protected override void RunActionOverride()
	{
		ColoniesGenerator.MoveEventFromStartedToScheduled(Event);
	}
}
