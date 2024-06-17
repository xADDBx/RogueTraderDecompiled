using System;
using System.Collections;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("183d933bebbe4022a26ce06db5d31b1e")]
public class CompanionImmortality : UnitFactComponentDelegate, IUnitDieHandler<EntitySubscriber>, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitDieHandler, EntitySubscriber>, IAreaActivationHandler, IHashable
{
	public float DisappearDelay = 6f;

	public GameObject DisappearFx;

	public ActionList Actions;

	public LocalizedString FakeDeathMessage;

	public void OnUnitDie()
	{
		if (!base.Owner.LifeState.IsDead)
		{
			return;
		}
		base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		CoroutineRunner.Start(DisappearAndResurrect(base.Owner));
		if (Game.Instance.Player.Party.HasItem(base.Owner))
		{
			Game.Instance.Player.RemoveCompanion(base.Owner, stayInGame: true);
		}
		if (FakeDeathMessage != null)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFakeDeathMessageHandler>)delegate(IUnitFakeDeathMessageHandler h)
			{
				h.HandleUnitFakeDeathMessage(FakeDeathMessage);
			}, isCheckRuntime: true);
		}
	}

	private void TryResurrect(BaseUnitEntity owner)
	{
		if (base.Owner.LifeState.IsDead && !owner.Destroyed)
		{
			owner.LifeState.Resurrect();
			if (Game.Instance.Player.Party.Contains(owner))
			{
				Game.Instance.Player.RemoveCompanion(owner);
			}
			owner.IsInGame = false;
		}
	}

	private IEnumerator DisappearAndResurrect(BaseUnitEntity owner)
	{
		GameObject fxPrefab = (DisappearFx ? DisappearFx : BlueprintRoot.Instance.SystemMechanics.FadeOutFx);
		UnitEntityView view = null;
		GameObject fx = null;
		TimeSpan startTime = default(TimeSpan);
		TimeSpan gameTime;
		do
		{
			if (owner.Destroyed)
			{
				yield break;
			}
			yield return null;
			if (view != owner.View)
			{
				view = owner.View;
				FxHelper.Destroy(fx);
				fx = FxHelper.SpawnFxOnEntity(fxPrefab, owner.View);
				startTime = Game.Instance.TimeController.GameTime;
			}
			gameTime = Game.Instance.TimeController.GameTime;
		}
		while ((gameTime - startTime).TotalSeconds < (double)DisappearDelay);
		TryResurrect(owner);
	}

	public void OnAreaActivated()
	{
		TryResurrect(base.Owner);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
