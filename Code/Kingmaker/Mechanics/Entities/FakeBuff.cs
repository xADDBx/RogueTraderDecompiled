using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public class FakeBuff : IBuff
{
	private LightweightUnitEntity m_Owner;

	private BlueprintBuff m_Blueprint;

	private GameObject m_ParticleEffect;

	MechanicEntity IBuff.Caster => m_Owner;

	TargetWrapper IBuff.Target => m_Owner;

	BlueprintAbilityFXSettings IBuff.FXSettings => m_Blueprint.FXSettings;

	public BlueprintBuff Blueprint => m_Blueprint;

	public static FakeBuff Create(LightweightUnitEntity owner, BlueprintBuff blueprint)
	{
		return new FakeBuff(owner, blueprint);
	}

	private FakeBuff(LightweightUnitEntity owner, BlueprintBuff blueprint)
	{
		m_Owner = owner;
		m_Blueprint = blueprint;
	}

	public void PlayFx()
	{
		GameObject prefab = m_Blueprint.FxOnStart.Load();
		EventBus.RaiseEvent(delegate(IBuffEffectHandler h)
		{
			h.OnBuffEffectApplied(this);
		});
		m_ParticleEffect = FxHelper.SpawnFxOnEntity(prefab, m_Owner.View);
	}

	public void Clear()
	{
		if ((bool)m_ParticleEffect)
		{
			EventBus.RaiseEvent(delegate(IBuffEffectHandler h)
			{
				h.OnBuffEffectRemoved(this);
			});
			FxHelper.Destroy(m_ParticleEffect);
			m_ParticleEffect = null;
		}
		m_Owner = null;
		m_Blueprint = null;
	}
}
