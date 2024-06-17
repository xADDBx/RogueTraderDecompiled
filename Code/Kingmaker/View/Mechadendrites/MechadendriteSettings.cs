using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.View.Mechadendrites;

public class MechadendriteSettings : MonoBehaviour
{
	public MechadendritesType MechadendritesType;

	private UnitEntityView m_UnitEntityView;

	private Character m_Character;

	public UnitAnimationManager AnimationManager { get; private set; }

	private void Awake()
	{
		AnimationManager = GetComponent<UnitAnimationManager>();
		m_UnitEntityView = GetComponentInParent<UnitEntityView>();
		if (!(m_UnitEntityView == null))
		{
			m_UnitEntityView.Data?.GetOptional<UnitPartMechadendrites>()?.RegisterMechadendrite(this);
		}
	}

	private void OnDestroy()
	{
		if (m_Character != null && m_Character.MechsAnimationManagers.Contains(AnimationManager))
		{
			m_Character.MechsAnimationManagers.Remove(AnimationManager);
		}
		if (m_UnitEntityView != null)
		{
			m_UnitEntityView.Data?.GetOptional<UnitPartMechadendrites>()?.UnregisterMechadendrite(this);
		}
	}
}
