using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("222d104176534e2885f7300d6ab23883")]
public class WarhammerDoInvisibleTarget : UnitBuffComponentDelegate, IUnitInvisibleHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	private static Dictionary<MechanicEntity, Buff> s_Targets = new Dictionary<MechanicEntity, Buff>();

	protected override void OnActivateOrPostLoad()
	{
		if (s_Targets.TryGetValue(base.Owner, out var value))
		{
			if (value.Active)
			{
				base.Buff.Remove();
				return;
			}
			s_Targets.Remove(base.Owner);
		}
		s_Targets.Add(base.Context.MainTarget.Entity, base.Buff);
		base.Owner.GetOrCreate<PartUnitInvisible>();
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitInvisibleHandler>)delegate(IUnitInvisibleHandler e)
		{
			e.HandleUnitUpdateInvisible();
		}, isCheckRuntime: true);
	}

	public void HandleUnitUpdateInvisible()
	{
	}

	public void RemoveUnitInvisible()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (s_Targets.TryGetValue(mechanicEntity, out var value))
		{
			s_Targets.Remove(mechanicEntity);
			value.Remove();
			base.Owner.Remove<PartUnitInvisible>();
			EventBus.RaiseEvent((IMechanicEntity)mechanicEntity, (Action<IUnitInvisibleHandler>)delegate(IUnitInvisibleHandler e)
			{
				e.HandleUnitUpdateInvisible();
			}, isCheckRuntime: true);
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
