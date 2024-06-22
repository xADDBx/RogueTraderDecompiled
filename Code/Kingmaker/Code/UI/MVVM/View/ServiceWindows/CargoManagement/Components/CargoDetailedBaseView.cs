using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoDetailedBaseView : VirtualListElementViewBase<CargoSlotVM>
{
	[Header("Title")]
	[SerializeField]
	private Image m_TypeIcon;

	[SerializeField]
	private Transform m_Content;

	[SerializeField]
	private TextMeshProUGUI m_TypeLabel;

	[SerializeField]
	private Image m_TooltipPlace;

	[Header("Slots")]
	[SerializeField]
	private ItemSlotsGroupView m_ItemSlotsGroup;

	[SerializeField]
	private InventorySlotView m_InventorySlotPrefab;

	[Header("Fill Details")]
	[SerializeField]
	private TextMeshProUGUI m_TotalFillValue;

	[SerializeField]
	private TextMeshProUGUI m_UnusableFillValue;

	[SerializeField]
	private Image m_UsableFillBar;

	[SerializeField]
	private Image m_UnusableFillBar;

	[Header("Other")]
	[SerializeField]
	private OwlcatButton m_BackButton;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	[SerializeField]
	private CanvasGroup m_BlinkCanvas;

	[SerializeField]
	protected GameObject m_FilledCargoMark;

	private bool m_IsInit;

	public new VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public ItemSlotsGroupView ItemSlotsGroup => m_ItemSlotsGroup;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_ItemSlotsGroup.Initialize(m_InventorySlotPrefab);
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_TypeIcon.sprite = base.ViewModel.TypeIcon;
		m_TypeLabel.text = base.ViewModel.TypeLabel;
		if (!m_IsInit)
		{
			Initialize();
		}
		AddDisposable(base.ViewModel.OnValueUpdate.Subscribe(delegate
		{
			OnCargoUpdate();
		}));
		AddDisposable(base.ViewModel.TotalFillValue.Subscribe(CargoFilling));
		AddDisposable(base.ViewModel.UnusableFillValue.Subscribe(delegate(int value)
		{
			m_UnusableFillValue.text = string.Format(UIStrings.Instance.CargoTexts.CargoUnusableFillValue, value);
		}));
		AddDisposable(base.ViewModel.TotalFillValue.CombineLatest(base.ViewModel.UnusableFillValue, (int total, int unusable) => new { total, unusable }).Subscribe(value =>
		{
			int num = value.total - value.unusable;
			m_UsableFillBar.fillAmount = (float)num / 100f;
		}));
		AddDisposable(m_TooltipPlace.SetTooltip(base.ViewModel.Tooltip));
		if (m_BlinkCanvas != null)
		{
			AddDisposable(base.ViewModel.NeedBlink.Subscribe(delegate
			{
				Blink(m_BlinkCanvas);
			}));
		}
		m_FadeAnimator.AppearAnimation();
		base.ViewModel.CreateItemSlotsGroup();
		m_ItemSlotsGroup.Bind(base.ViewModel.ItemSlotsGroup);
		m_LayoutSettings.SetDirty();
	}

	protected void OnCargoUpdate()
	{
		m_LayoutSettings.SetDirty();
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	protected void CargoFilling(int value)
	{
		m_FilledCargoMark.SetActive(value >= 100);
		m_TotalFillValue.text = UIConfig.Instance.PercentHelper.AddPercentTo(value);
	}

	public void Blink(CanvasGroup canvasGroup)
	{
		UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
	}
}
