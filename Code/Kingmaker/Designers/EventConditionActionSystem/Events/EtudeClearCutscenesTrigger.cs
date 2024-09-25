using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EtudeClearCutscenesTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("95443bd6bf37415081321e7efef5a06f")]
public class EtudeClearCutscenesTrigger : EtudeBracketTrigger, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool AlreadyProcessedActivation;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AlreadyProcessedActivation);
			return result;
		}
	}

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
		foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
		{
			if (!cutscene.IsFinished && !cutscene.Paused && !cutscene.Cutscene.LockControl && HasPartyUnitControlled(cutscene))
			{
				cutscene.Stop();
			}
		}
	}

	private static bool HasPartyUnitControlled(CutscenePlayerData cutscene)
	{
		foreach (AbstractUnitEntity currentControlledUnit in cutscene.GetCurrentControlledUnits())
		{
			if (currentControlledUnit is BaseUnitEntity baseUnitEntity && baseUnitEntity.CombatGroup.IsPlayerParty)
			{
				return true;
			}
		}
		return false;
	}

	public void OnEtudesUpdate()
	{
		MaybeTrigger();
	}

	public void OnAreaActivated()
	{
		MaybeTrigger();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
