using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class AreaEffectUnboundPart : EntityPart<AreaEffectEntity>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, IEventTag<IInGameHandler, EntitySubscriber>, AreaEffectEntity.IUnitWithinBoundsHandler, IHashable
{
	public HashSet<UnitReference> Entered { get; } = new HashSet<UnitReference>();


	public HashSet<UnitReference> Exited { get; } = new HashSet<UnitReference>();


	public void ClearDelta()
	{
		Entered.Clear();
		Exited.Clear();
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			Entered.Add(baseUnitEntity.FromBaseUnitEntity());
		}
	}

	public void HandleUnitDestroyed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			Exited.Add(baseUnitEntity.FromBaseUnitEntity());
		}
	}

	public void HandleUnitDeath()
	{
	}

	public void HandleObjectInGameChanged()
	{
		if (base.Owner.IsInGame)
		{
			AddAll();
		}
		else
		{
			RemoveAll();
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		AddAll();
	}

	protected override void OnViewWillDetach()
	{
		base.OnViewWillDetach();
		RemoveAll();
	}

	private void AddAll()
	{
		Exited.Clear();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			Entered.Add(allBaseUnit.FromBaseUnitEntity());
		}
	}

	private void RemoveAll()
	{
		Entered.Clear();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			Exited.Add(allBaseUnit.FromBaseUnitEntity());
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
