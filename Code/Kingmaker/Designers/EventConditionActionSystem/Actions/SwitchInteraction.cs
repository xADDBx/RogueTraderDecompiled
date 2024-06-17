using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("675865eb3bfd09a46beb0a7283774f0f")]
public class SwitchInteraction : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool EnableIfAlreadyDisabled;

	public bool DisableIfAlreadyEnabled;

	public override string GetDescription()
	{
		return $"Включает или выключает кликабельность для мапобжекта {MapObject}.\n" + $"Включить, если выключена: {EnableIfAlreadyDisabled}\n" + $"Выключить, если включена: {DisableIfAlreadyEnabled}";
	}

	public override string GetCaption()
	{
		string arg = ((!EnableIfAlreadyDisabled && !DisableIfAlreadyEnabled) ? "Do nothing with" : ((!EnableIfAlreadyDisabled || !DisableIfAlreadyEnabled) ? (EnableIfAlreadyDisabled ? "Enable" : "Disable") : "Switch"));
		return $"{arg} interactions in {MapObject}";
	}

	public override void RunAction()
	{
		foreach (InteractionPart interaction in MapObject.GetValue().Interactions)
		{
			if (interaction.Enabled && DisableIfAlreadyEnabled)
			{
				interaction.Enabled = false;
			}
			if (!interaction.Enabled && EnableIfAlreadyDisabled)
			{
				interaction.Enabled = true;
			}
		}
	}
}
