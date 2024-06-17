using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarSlotWeaponAbilityView : ActionBarBaseSlotView
{
	[Header("Ammo Cost")]
	[SerializeField]
	private GameObject m_AmmoCostContainer;

	protected VisibilityController m_AmmoCostContainerVisibility;

	[SerializeField]
	private TextMeshProUGUI m_AmmoCost;

	[Header("Reload")]
	[SerializeField]
	protected GameObject m_ReloadAmmoContainer;

	protected VisibilityController m_ReloadAmmoContainerVisibility;

	[SerializeField]
	private TextMeshProUGUI m_ReloadAmmoText;

	[SerializeField]
	private Slider m_ReloadAmmoBar;

	[SerializeField]
	private GameObject m_CurrentAmmoContainer;

	protected VisibilityController m_CurrentAmmoContainerVisibility;

	[SerializeField]
	private TextMeshProUGUI m_CurrentAmmoText;

	protected ReactiveProperty<bool> m_ShowAmmoContainer = new ReactiveProperty<bool>();

	protected override void Awake()
	{
		base.Awake();
		m_AmmoCostContainerVisibility = VisibilityController.Control(m_AmmoCostContainer);
		m_ReloadAmmoContainerVisibility = VisibilityController.Control(m_ReloadAmmoContainer);
		m_CurrentAmmoContainerVisibility = VisibilityController.Control(m_CurrentAmmoContainer);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_AmmoCostContainer != null)
		{
			AddDisposable(base.ViewModel.AmmoCost.Subscribe(SetAmmoCost));
		}
		AddDisposable(m_ShowAmmoContainer.Subscribe(delegate(bool show)
		{
			m_ReloadAmmoContainerVisibility?.SetVisible(show);
		}));
		AddDisposable(base.ViewModel.IsReload.Subscribe(delegate(bool show)
		{
			m_CurrentAmmoContainerVisibility?.SetVisible(show);
		}));
		AddDisposable(base.ViewModel.IsReload.CombineLatest(base.ViewModel.CurrentAmmo, (bool isReload, int current) => new { isReload, current }).Subscribe(val =>
		{
			m_ShowAmmoContainer.Value = val.isReload && val.current >= base.ViewModel.MaxWeaponAbilityAmmo && Game.Instance.IsControllerMouse;
		}));
		AddDisposable(base.ViewModel.CurrentAmmo.CombineLatest(base.ViewModel.MaxAmmo, (int current, int max) => new { current, max }).Subscribe(val =>
		{
			if (base.ViewModel.IsReload.Value && (bool)m_ReloadAmmoContainer)
			{
				TextMeshProUGUI reloadAmmoText = m_ReloadAmmoText;
				string text2 = (m_CurrentAmmoText.text = $"{val.current}/{val.max}");
				reloadAmmoText.text = text2;
				m_ReloadAmmoBar.value = ((val.max != 0) ? ((float)val.current / (float)val.max) : 0f);
			}
		}));
	}

	private void SetAmmoCost(int value)
	{
		bool flag = value > 0;
		m_AmmoCostContainerVisibility.SetVisible(flag);
		if (flag)
		{
			m_AmmoCost.text = value.ToString();
		}
	}
}
