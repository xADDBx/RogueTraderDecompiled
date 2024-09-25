using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
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

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[ComponentName("Events/EtudeInvokeActionsDelayed")]
[TypeId("e75e3ec07fb24b1b8146a0a83245e42f")]
public class EtudeInvokeActionsDelayed : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	[JsonObject]
	public class EtudeInvokeActionDelayedData : EntityFactComponent<EtudesSystem, EtudeInvokeActionsDelayed>, ITimedEvent, ISubscriber, IHashable
	{
		public class SavableData : IEntityFactComponentSavableData, IHashable
		{
			[JsonProperty]
			public bool Executed;

			[JsonProperty]
			public TimeSpan LastTickTime;

			[JsonProperty]
			public TimeSpan TimeRemaining;

			public override Hash128 GetHash128()
			{
				Hash128 result = default(Hash128);
				Hash128 val = base.GetHash128();
				result.Append(ref val);
				result.Append(ref Executed);
				result.Append(ref LastTickTime);
				result.Append(ref TimeRemaining);
				return result;
			}
		}

		protected override void OnInitialize()
		{
			RequestSavableData<SavableData>().TimeRemaining = TimeSpan.FromDays(base.Settings.m_Days);
		}

		protected override void OnActivateOrPostLoad()
		{
			RequestSavableData<SavableData>().LastTickTime = Game.Instance.TimeController.GameTime;
		}

		protected override void OnDeactivate()
		{
			HandleTimePassed();
		}

		public void HandleTimePassed()
		{
			SavableData savableData = RequestSavableData<SavableData>();
			Etude etude = (Etude)base.Fact;
			if (!savableData.Executed && etude.IsPlaying)
			{
				TimeSpan gameTime = Game.Instance.TimeController.GameTime;
				TimeSpan timeSpan = gameTime - savableData.LastTickTime;
				savableData.LastTickTime = gameTime;
				savableData.TimeRemaining -= timeSpan;
				if (!(savableData.TimeRemaining.TotalMilliseconds > 0.0))
				{
					base.Settings.m_ActionList?.Run();
					savableData.Executed = true;
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

	[SerializeField]
	[Tooltip("How much in-game days should pass for ActionList to be invoked")]
	private int m_Days;

	[SerializeField]
	[Tooltip("Actions to invoke after required amount of days has passed")]
	private ActionList m_ActionList;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new EtudeInvokeActionDelayedData();
	}
}
