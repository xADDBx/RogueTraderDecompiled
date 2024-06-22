using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EtudePlayCutsceneTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("7939f83ef69d4ea4881639b91e078d72")]
public class EtudePlayCutsceneTrigger : EtudeBracketTrigger, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool AlreadyTriggered;

		[JsonProperty]
		public bool AlreadyProcessedActivation;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AlreadyTriggered);
			result.Append(ref AlreadyProcessedActivation);
			return result;
		}
	}

	[SerializeField]
	private bool m_Once;

	public ConditionsChecker Conditions = new ConditionsChecker();

	[SerializeReference]
	public PlayCutscene[] CutsceneActions;

	protected override void OnActivate()
	{
		RequestSavableData<SavableData>().AlreadyProcessedActivation = false;
	}

	private void MaybeTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (savableData.AlreadyProcessedActivation || Game.Instance.Player.EtudesSystem.GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).Any((BlueprintAreaMechanics mechanics) => !mechanics.IsSceneLoadedNow()))
		{
			return;
		}
		savableData.AlreadyProcessedActivation = true;
		if ((!m_Once || !savableData.AlreadyTriggered) && Conditions.Check())
		{
			PlayCutscene[] cutsceneActions = CutsceneActions;
			for (int i = 0; i < cutsceneActions.Length; i++)
			{
				cutsceneActions[i].Run();
			}
			savableData.AlreadyTriggered = true;
		}
	}

	public void OnEtudesUpdate()
	{
		MaybeTrigger();
	}

	public void OnAreaActivated()
	{
		MaybeTrigger();
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		StopAllCutscenes();
	}

	protected override void OnExit()
	{
		base.OnExit();
		StopAllCutscenes();
	}

	private void StopAllCutscenes()
	{
		PlayCutscene[] cutsceneActions = CutsceneActions;
		for (int i = 0; i < cutsceneActions.Length; i++)
		{
			CutscenePlayerData cutsceneData = cutsceneActions[i].CutsceneData;
			if (cutsceneData != null && !cutsceneData.IsFinished)
			{
				cutsceneData.Stop();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
