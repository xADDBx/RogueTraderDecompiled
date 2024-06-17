using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("1a257c2f62219924aa445b0058430d08")]
public abstract class UnitBuffComponentDelegate : UnitFactComponentDelegate, IHashable
{
	public class UnitBuffComponentRuntime : ComponentRuntime, IBuffRemoved, IHashable
	{
		private UnitBuffComponentDelegate Delegate => (UnitBuffComponentDelegate)base.SourceBlueprintComponent;

		void IBuffRemoved.OnRemoved()
		{
			using (RequestEventContext())
			{
				try
				{
					Delegate.OnRemoved();
				}
				catch (Exception ex)
				{
					PFLog.EntityFact.Exception(ex);
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

	protected Buff Buff => base.Fact as Buff;

	protected virtual void OnRemoved()
	{
	}

	public override EntityFactComponent CreateRuntimeFactComponent()
	{
		return new UnitBuffComponentRuntime();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
