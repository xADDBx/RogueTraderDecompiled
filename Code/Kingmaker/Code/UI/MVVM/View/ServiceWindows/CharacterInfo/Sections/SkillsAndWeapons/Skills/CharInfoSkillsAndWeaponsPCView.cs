using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;

public class CharInfoSkillsAndWeaponsPCView : CharInfoSkillsAndWeaponsBaseView
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_SkillsButton;

	[SerializeField]
	private OwlcatMultiButton m_WeaponsButton;

	[SerializeField]
	private Transform m_TabSelector;

	private float m_CurrentAngle;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_SkillsButton != null)
		{
			m_SkillsButton.SetActiveLayer((CurrentSection.Value == CharInfoComponentType.Skills) ? 1 : 0);
		}
		if (m_WeaponsButton != null)
		{
			m_WeaponsButton.SetActiveLayer((CurrentSection.Value == CharInfoComponentType.Weapons) ? 1 : 0);
		}
		m_CurrentAngle = ((CurrentSection.Value == CharInfoComponentType.Skills) ? 0f : 180f);
		if (m_TabSelector != null)
		{
			m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
		}
		AddDisposable(m_SkillsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CurrentSection.Value = CharInfoComponentType.Skills;
			m_SkillsButton.SetActiveLayer(1);
			m_WeaponsButton.SetActiveLayer(0);
			m_CurrentAngle = 0f;
			if (m_TabSelector != null)
			{
				m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
			}
		}));
		AddDisposable(m_WeaponsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CurrentSection.Value = CharInfoComponentType.Weapons;
			m_SkillsButton.SetActiveLayer(0);
			m_WeaponsButton.SetActiveLayer(1);
			m_CurrentAngle = 180f;
			if (m_TabSelector != null)
			{
				m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
			}
		}));
	}

	protected override void InternalBindSection(CharInfoComponentType section)
	{
		if (section == CharInfoComponentType.Skills)
		{
			m_WeaponsBlockPCView.UnbindSection();
			m_SkillsBlockPCView.BindSection(base.ViewModel.SkillsBlockVM);
		}
		else
		{
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.BindSection(base.ViewModel.WeaponsBlockVM);
		}
	}
}
