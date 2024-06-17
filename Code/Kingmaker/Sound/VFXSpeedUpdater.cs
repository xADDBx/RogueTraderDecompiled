using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Sound;

public class VFXSpeedUpdater : MonoBehaviour, IVisualWeaponStateChangeHandle, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public enum WeaponVisualState
	{
		InHand,
		OutHand,
		InAttack
	}

	private static int s_SpeedPropertyId = Shader.PropertyToID("Speed");

	public float InHandSpeed;

	public float OutHandSpeed;

	public float AttackSpeed;

	private WeaponVisualState m_CurrentState = WeaponVisualState.OutHand;

	private VisualEffect m_VisualEffect;

	[UsedImplicitly]
	private void Awake()
	{
		m_VisualEffect = GetComponentInChildren<VisualEffect>();
		if (m_VisualEffect != null)
		{
			EventBus.Subscribe(this);
			UpdateVisualEffectSpeedProperty();
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	public void VisualWeaponStateChangeHandle(WeaponVisualState weaponVisualState, GameObject visualModel)
	{
		if (!(m_VisualEffect == null) && !(visualModel != base.gameObject) && m_CurrentState != weaponVisualState)
		{
			m_CurrentState = weaponVisualState;
			UpdateVisualEffectSpeedProperty();
		}
	}

	private void UpdateVisualEffectSpeedProperty()
	{
		float f = m_CurrentState switch
		{
			WeaponVisualState.InHand => InHandSpeed, 
			WeaponVisualState.OutHand => OutHandSpeed, 
			WeaponVisualState.InAttack => AttackSpeed, 
			_ => OutHandSpeed, 
		};
		m_VisualEffect.SetFloat(s_SpeedPropertyId, f);
	}
}
