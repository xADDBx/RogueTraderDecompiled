using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[AllowedOn(typeof(BlueprintComponentList))]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[TypeId("4d95171e41fd4447a47bf00f82bb5070")]
public abstract class EtudeBracketTrigger : EntityFactComponentDelegate<EtudesSystem>, IHashable
{
	public class EtudeBracketRuntime : ComponentRuntime, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler, IHashable
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

		private bool m_NeedResume;

		public Etude Etude => (Etude)base.Fact;

		private EtudeBracketTrigger Delegate => (EtudeBracketTrigger)base.SourceBlueprintComponent;

		private bool LinkedAreaUnavailable => IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.CurrentlyLoadedAreaPart);

		private bool LinkedAreaUnavailableOnExit
		{
			get
			{
				if (IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.CurrentlyLoadedAreaPart))
				{
					return IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.Player.EtudesSystem.AreaPartBeingExited);
				}
				return false;
			}
		}

		private static bool IsLinkedAreaUnavailable(BlueprintEtude etude, BlueprintAreaPart area)
		{
			if (!etude.HasLinkedAreaPart || etude.IsLinkedAreaPart(area))
			{
				if (!etude.Parent.IsEmpty())
				{
					return IsLinkedAreaUnavailable(etude.Parent, area);
				}
				return false;
			}
			return true;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			RequestSavableData<SavableData>().AlreadyProcessedActivation = false;
			m_NeedResume = false;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			SavableData savableData = RequestSavableData<SavableData>();
			if (savableData.AlreadyProcessedActivation)
			{
				savableData.AlreadyProcessedActivation = false;
				m_NeedResume = false;
				Exit();
			}
		}

		private void MaybeEnter()
		{
			if (LinkedAreaUnavailable)
			{
				return;
			}
			SavableData savableData = RequestSavableData<SavableData>();
			if (savableData.AlreadyProcessedActivation)
			{
				if (m_NeedResume)
				{
					m_NeedResume = false;
					Resume();
				}
			}
			else
			{
				savableData.AlreadyProcessedActivation = true;
				Enter();
			}
		}

		public void OnEtudesUpdate()
		{
			MaybeEnter();
		}

		public void OnAreaActivated()
		{
			MaybeEnter();
		}

		protected override void OnPostLoad()
		{
			base.OnPostLoad();
			SavableData savableData = RequestSavableData<SavableData>();
			if (base.Fact.Active && savableData.AlreadyProcessedActivation)
			{
				if (LinkedAreaUnavailable)
				{
					m_NeedResume = true;
				}
				else
				{
					Resume();
				}
			}
		}

		private void Enter()
		{
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnEnter();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
			}
		}

		private void Exit()
		{
			try
			{
				if (LinkedAreaUnavailableOnExit)
				{
					PFLog.Etudes.ErrorWithReport(Etude.Blueprint, "EtudeBracketTrigger.OnExit: skip, because of linked area is unavailable. Probably etude was changed by designer and should be deactivated after game loaded.");
					return;
				}
				using (RequestEventContext())
				{
					Delegate.OnExit();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
			}
		}

		private void Resume()
		{
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnResume();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
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

	public virtual bool RequireLinkedArea => false;

	protected static Etude Etude => (Etude)ComponentEventContext.CurrentRuntime.Fact;

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnResume()
	{
	}

	public override EntityFactComponent CreateRuntimeFactComponent()
	{
		return new EtudeBracketRuntime();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
