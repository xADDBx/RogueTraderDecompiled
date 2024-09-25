using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public class WeaponAbilitySlotBaseView : ActionBarBaseSlotView
{
	[SerializeField]
	private Image m_MicroAbilityIcon;

	private VisibilityController m_MicroAbilityIconVisibility;

	protected override void Awake()
	{
		base.Awake();
		m_MicroAbilityIconVisibility = VisibilityController.Control(m_MicroAbilityIcon);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.MicroAbilityIcon.Subscribe(delegate(Sprite value)
		{
			m_MicroAbilityIconVisibility.SetVisible(value != null);
			m_MicroAbilityIcon.sprite = value;
		}));
	}
}
