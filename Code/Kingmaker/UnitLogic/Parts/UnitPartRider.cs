using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartRider : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityPartRef<BaseUnitEntity, UnitPartSaddled> m_SaddledUnitRef;

	public BaseUnitEntity SaddledUnit => m_SaddledUnitRef.Entity;

	public void Mount([NotNull] BaseUnitEntity target)
	{
	}

	public void Dismount()
	{
	}

	public void DismountForce()
	{
		if (SaddledUnit != null)
		{
			ClearSaddledUnit();
			SetAvoidanceEnabled(enabled: true);
			Clear(toggleAbilityImmediately: true);
		}
	}

	private void ClearSaddledUnit()
	{
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitMountHandler>)delegate(IUnitMountHandler h)
		{
			h.HandleUnitDismount(m_SaddledUnitRef.Entity);
		}, isCheckRuntime: true);
		m_SaddledUnitRef.EntityPart?.Clear();
		m_SaddledUnitRef.Entity?.Remove<UnitPartSaddled>();
		m_SaddledUnitRef = default(EntityPartRef<BaseUnitEntity, UnitPartSaddled>);
	}

	private void Clear(bool toggleAbilityImmediately = false)
	{
	}

	private void SetAvoidanceEnabled(bool enabled)
	{
		UnitMovementAgentBase unitMovementAgentBase = base.Owner.View.Or(null)?.MovementAgent;
		if (unitMovementAgentBase != null)
		{
			unitMovementAgentBase.AvoidanceDisabled = !enabled;
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		SetAvoidanceEnabled(enabled: false);
	}

	protected override void OnDetach()
	{
		SetAvoidanceEnabled(enabled: true);
	}

	protected override void OnViewDidAttach()
	{
		SetAvoidanceEnabled(enabled: false);
	}

	protected override void OnViewWillDetach()
	{
		SetAvoidanceEnabled(enabled: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityPartRef<BaseUnitEntity, UnitPartSaddled> obj = m_SaddledUnitRef;
		Hash128 val2 = StructHasher<EntityPartRef<BaseUnitEntity, UnitPartSaddled>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
