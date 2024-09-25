using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EtudePlayTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("fec4340333b7d1f4189d73aa8ff10eee")]
public class EtudePlayTrigger : EtudeBracketTrigger, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler, IHashable
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

	[SerializeField]
	private bool m_IgnoreAlreadyProcessedActivation;

	public ConditionsChecker Conditions = new ConditionsChecker();

	public ActionList Actions;

	protected override void OnActivate()
	{
		RequestSavableData<SavableData>().AlreadyProcessedActivation = false;
	}

	private void MaybeTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if ((m_IgnoreAlreadyProcessedActivation || !savableData.AlreadyProcessedActivation) && !Game.Instance.Player.EtudesSystem.GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).Any((BlueprintAreaMechanics mechanics) => !mechanics.IsSceneLoadedNow()))
		{
			savableData.AlreadyProcessedActivation = true;
			if ((!m_Once || !savableData.AlreadyTriggered) && Conditions.Check())
			{
				Actions.Run();
				savableData.AlreadyTriggered = true;
			}
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

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
