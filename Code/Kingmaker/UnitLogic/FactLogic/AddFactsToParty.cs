using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowMultipleComponents]
[TypeId("c0692276882a413b95b13da9c5ffdf23")]
public class AddFactsToParty : MechanicEntityFactComponentDelegate, IAreaHandler, ISubscriber, ICompanionStateChanged<EntitySubscriber>, ICompanionStateChanged, ISubscriber<IMechanicEntity>, IEventTag<ICompanionStateChanged, EntitySubscriber>, IAcceptChangeGroupHandler, IHashable
{
	public RestrictionCalculator Restriction;

	public bool DoNotAddToSelf;

	public bool AddIfOwnerNotInParty;

	public bool AddToRemoteCompanions;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintMechanicEntityFact.Reference[] m_Facts = new BlueprintMechanicEntityFact.Reference[0];

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected override void OnActivate()
	{
		UpdateFacts();
	}

	protected override void OnDeactivate()
	{
		RemoveFacts();
	}

	protected override void OnApplyPostLoadFixes()
	{
		UpdateFacts();
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			List<EntityFact> list = TempList.Get<EntityFact>();
			foreach (EntityFact item in allCrossSceneUnit.Facts.List)
			{
				if (item.IsFrom(base.Fact, this) && !Facts.HasReference(item.Blueprint))
				{
					list.Add(item);
				}
			}
			foreach (EntityFact item2 in list)
			{
				allCrossSceneUnit.Facts.Remove(item2);
			}
		}
	}

	public void OnAreaBeginUnloading()
	{
		if (base.Owner.HoldingState != Game.Instance.Player.CrossSceneState)
		{
			RemoveFacts();
		}
	}

	public void OnAreaDidLoad()
	{
		if (base.Owner.HoldingState != Game.Instance.Player.CrossSceneState)
		{
			UpdateFacts();
		}
	}

	public void HandleCompanionStateChanged()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity == base.Owner)
		{
			UpdateFacts();
		}
		else if (IsSuitable(mechanicEntity))
		{
			AddFactsIfRestrictionPassed(mechanicEntity);
		}
		else
		{
			RemoveFacts(mechanicEntity);
		}
	}

	public void HandleAcceptChangeGroup()
	{
		UpdateFacts();
	}

	private void UpdateFacts()
	{
		bool flag = Game.Instance.Player.MainCharacter != Game.Instance.Player.MainCharacterOriginal;
		bool flag2;
		switch (base.Owner.GetCompanionOptional()?.State)
		{
		case CompanionState.InParty:
		case CompanionState.InPartyDetached:
			flag2 = true;
			break;
		default:
			flag2 = false;
			break;
		}
		bool flag3 = flag2;
		if (flag)
		{
			CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
			flag3 = companionState.HasValue && companionState.GetValueOrDefault() == CompanionState.InParty;
		}
		if (!AddIfOwnerNotInParty && !flag3)
		{
			RemoveFacts();
			return;
		}
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			if (IsSuitable(allCrossSceneUnit))
			{
				AddFactsIfRestrictionPassed(allCrossSceneUnit);
			}
			else
			{
				RemoveFacts(allCrossSceneUnit);
			}
		}
	}

	private void AddFactsIfRestrictionPassed(MechanicEntity entity)
	{
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			if (!entity.Facts.Contains(fact) && Restriction.IsPassed(base.Fact, entity))
			{
				entity.AddFact(fact)?.AddSource(base.Fact, this);
			}
		}
	}

	private void RemoveFacts()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			RemoveFacts(allCrossSceneUnit);
		}
	}

	private void RemoveFacts(MechanicEntity entity)
	{
		entity.Facts.RemoveAll((MechanicEntityFact i) => i.IsFrom(base.Fact));
	}

	private bool IsSuitable(MechanicEntity entity)
	{
		if (DoNotAddToSelf && entity == base.Owner)
		{
			return false;
		}
		CompanionState? companionState = entity.GetCompanionOptional()?.State;
		bool flag;
		switch (companionState)
		{
		case CompanionState.InParty:
		case CompanionState.InPartyDetached:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			if (companionState.HasValue && companionState.GetValueOrDefault() == CompanionState.Remote)
			{
				return AddToRemoteCompanions;
			}
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
