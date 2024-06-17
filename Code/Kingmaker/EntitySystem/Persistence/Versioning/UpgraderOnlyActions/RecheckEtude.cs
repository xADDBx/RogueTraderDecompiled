using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("96d13650ba485a8429784fb9d95de890")]
internal class RecheckEtude : PlayerUpgraderOnlyAction
{
	public BlueprintEtudeReference Etude;

	[SerializeField]
	private bool m_RedoOnceTriggers;

	protected override void RunActionOverride()
	{
		BlueprintEtude blueprintEtude = Etude.Get();
		if (!blueprintEtude)
		{
			return;
		}
		Etude etude = Game.Instance.Player.EtudesSystem.Etudes.Get(blueprintEtude);
		if (etude == null || !etude.IsPlaying)
		{
			return;
		}
		etude.Deactivate();
		if (m_RedoOnceTriggers)
		{
			etude.CallComponentsWithRuntime(delegate(EtudePlayTrigger bc, EntityFactComponent rt)
			{
				((EntityFactComponentDelegate<EtudesSystem>.ComponentRuntime)rt).RequestSavableData<EtudePlayTrigger.SavableData>().AlreadyTriggered = false;
			});
		}
		Game.Instance.Player.EtudesSystem.UpdateEtudes();
	}

	public override string GetCaption()
	{
		return "Recheck " + Etude.NameSafe();
	}

	public override string GetDescription()
	{
		return "Stops " + Etude.NameSafe() + " and updates etude system. Will restart the etude if required.";
	}
}
