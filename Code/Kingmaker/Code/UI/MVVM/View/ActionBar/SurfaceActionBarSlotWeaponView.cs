using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarSlotWeaponView : ViewBase<ItemSlotVM>
{
	[SerializeField]
	private Image m_WeaponIcon;

	[SerializeField]
	private Color m_FakeColor;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Sprite m_UnarmedIcon;

	[SerializeField]
	private Image m_FakeBackground;

	[SerializeField]
	private TextMeshProUGUI m_AmmoCountText;

	private BoolReactiveProperty m_FakeSlot = new BoolReactiveProperty(initialValue: false);

	private RectTransform m_TooltipCustomPlace;

	protected List<Vector2> m_TooltipPriorityPivots;

	protected RectTransform TooltipPlace
	{
		get
		{
			if (!(m_TooltipCustomPlace != null))
			{
				return base.transform as RectTransform;
			}
			return m_TooltipCustomPlace;
		}
	}

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite icon)
		{
			m_WeaponIcon.sprite = ((icon != null) ? icon : m_UnarmedIcon);
		}));
		AddDisposable(m_FakeSlot.Subscribe(delegate(bool val)
		{
			m_WeaponIcon.color = (val ? m_FakeColor : m_NormalColor);
			m_FakeBackground.Or(null)?.gameObject.SetActive(val || !base.ViewModel.HasItem);
		}));
		AddDisposable(base.ViewModel.Item.Subscribe(delegate
		{
			if ((bool)m_AmmoCountText)
			{
				ItemEntityWeapon itemWeapon = base.ViewModel.ItemWeapon;
				bool flag = itemWeapon != null && itemWeapon.Blueprint.WarhammerMaxAmmo > 0;
				m_AmmoCountText.gameObject.SetActive(flag);
				if (flag)
				{
					m_AmmoCountText.text = $"{itemWeapon.CurrentAmmo}/{itemWeapon.Blueprint.WarhammerMaxAmmo}";
				}
			}
		}));
	}

	public void SetFakeMode(bool state)
	{
		m_FakeSlot.Value = state;
	}

	public void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_TooltipCustomPlace = rectTransform;
		m_TooltipPriorityPivots = pivots;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
