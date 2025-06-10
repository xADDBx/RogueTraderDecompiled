using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("05a79e0f416f45e5a96ce2156a15828e")]
public class CasterTurnFactActivator : MechanicEntityFactComponentDelegate, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IContinueTurnHandler, ITurnStartHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, IInterruptTurnContinueHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		[CanBeNull]
		public string FactId;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(FactId);
			return result;
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_FactBlueprint;

	public bool IncludingInterrupts;

	public BlueprintMechanicEntityFact FactBlueprint => m_FactBlueprint;

	protected override void OnActivate()
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController != null && turnController.TbActive && turnController.CurrentUnit == base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	protected override void OnDeactivate()
	{
		UpdateFact(add: false);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (IncludingInterrupts && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: false);
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (IncludingInterrupts && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: false);
		}
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		if (IncludingInterrupts && EventInvokerExtensions.MechanicEntity == base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	private void UpdateFact(bool add)
	{
		if (!base.Owner.IsPreview)
		{
			MechanicEntity owner = base.Owner;
			SavableData savableData = RequestSavableData<SavableData>();
			if (add && savableData.FactId == null)
			{
				MechanicEntityFact mechanicEntityFact = FactBlueprint.CreateFact(base.Context, owner, default(BuffDuration));
				owner.Facts.Add(mechanicEntityFact);
				savableData.FactId = mechanicEntityFact.UniqueId;
			}
			else if (!add && savableData.FactId != null)
			{
				owner.Facts.RemoveById(savableData.FactId);
				savableData.FactId = null;
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
