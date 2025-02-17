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
[TypeId("c7894d09190e4f30b6455a035db59a28")]
public class OutOfCasterTurnFactActivator : MechanicEntityFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
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

	public BlueprintMechanicEntityFact FactBlueprint => m_FactBlueprint;

	protected override void OnActivate()
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController != null && turnController.TbActive && turnController.CurrentUnit != base.Context.MaybeCaster)
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
		if (isTurnBased)
		{
			UpdateFact(EventInvokerExtensions.MechanicEntity != base.Context.MaybeCaster);
		}
	}

	private void UpdateFact(bool add)
	{
		MechanicEntity owner = base.Owner;
		if (!owner.IsPreview)
		{
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
