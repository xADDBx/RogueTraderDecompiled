using System.Collections;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d9506448576f409eb799a06b6ba7634e")]
[ClassInfoBox("Unit pretends to be dead. Instant prone animation without ticking")]
public class FakeDeathAnimationState : UnitBuffComponentDelegate, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public class TransientData : IEntityFactComponentTransientData
	{
		public bool IsFakingDeath;

		public Coroutine FreezeCoroutine;
	}

	protected override void OnActivate()
	{
		if (!base.IsReapplying)
		{
			TryFake();
		}
	}

	protected override void OnDeactivate()
	{
		if (base.IsReapplying)
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		TryRemoveProne();
		if (transientData.FreezeCoroutine != null)
		{
			CoroutineRunner.Stop(transientData.FreezeCoroutine);
			transientData.FreezeCoroutine = null;
		}
		if (!(base.Owner.View.AnimationManager == null))
		{
			if (!base.Owner.View.Animator.enabled)
			{
				base.Owner.View.Animator.enabled = true;
			}
			base.Owner.View.AnimationManager.Disabled = false;
			base.Owner.View.AnimationManager.IsProne = false;
			base.Owner.View.AnimationManager.IsDead = false;
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (base.Owner.HoldingState != null)
		{
			TryFake();
		}
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && base.Owner == baseUnitEntity)
		{
			TryFake();
		}
	}

	private void TryFake()
	{
		TransientData transientData = RequestTransientData<TransientData>();
		if (!transientData.IsFakingDeath && !(base.Owner.View.Or(null)?.AnimationManager == null))
		{
			TryApplyProne();
			if (!base.Owner.View.AnimationManager.IsProne || base.Owner.View.AnimationManager.IsGoingProne)
			{
				base.Owner.View.AnimationManager.IsProne = true;
				base.Owner.View.AnimationManager.Tick(0f);
			}
			transientData.FreezeCoroutine = CoroutineRunner.Start(TurnOffAnimatorAfterDelay((UnitEntity)base.Owner));
			transientData.IsFakingDeath = true;
		}
	}

	private IEnumerator TurnOffAnimatorAfterDelay(UnitEntity unit)
	{
		yield return new WaitForSeconds(1f);
		yield return new WaitWhile(() => unit.View.AnimationManager.Or(null)?.IsGoingProne ?? false);
		yield return null;
		if (unit.View.AnimationManager != null)
		{
			unit.View.AnimationManager.Disabled = true;
		}
	}

	private void TryApplyProne()
	{
		if (!base.Owner.State.HasCondition(UnitCondition.Prone))
		{
			base.Owner.State.AddCondition(UnitCondition.Prone, base.Fact);
		}
	}

	private void TryRemoveProne()
	{
		if (base.Owner.State.HasCondition(UnitCondition.Prone))
		{
			base.Owner.State.RemoveCondition(UnitCondition.Prone, base.Fact);
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
