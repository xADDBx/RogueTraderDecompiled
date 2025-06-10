using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

public class DestructibleByUnitCollisionPart : ViewBasedPart<DestructibleByUnitCollisionSettings>, IAreaHandler, ISubscriber, IHashable
{
	private PartHealth m_Health;

	private PartDestructionStagesManager m_DestructionStagesManager;

	private void SubscribeOnUnitEnteredScriptZoneEvent()
	{
		if ((bool)base.Settings.CollisionZone)
		{
			base.Settings.CollisionZone.OnUnitEntered.AddListener(delegate
			{
				TryDestructOwner();
			});
		}
	}

	private void TryDestructOwner()
	{
		if (base.Owner.IsInGame && m_DestructionStagesManager.Stage != DestructionStage.Destroyed)
		{
			m_Health.SetDamage(m_Health.MaxHitPoints);
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		m_Health = base.Owner.GetRequired<PartHealth>();
		m_DestructionStagesManager = base.Owner.GetRequired<PartDestructionStagesManager>();
	}

	public void OnAreaDidLoad()
	{
		SubscribeOnUnitEnteredScriptZoneEvent();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
