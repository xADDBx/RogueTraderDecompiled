using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Loot;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorTransitionWindowView : ViewBase<VendorTransitionWindowVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[SerializeField]
	private ItemSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private LootInventorySlotView m_SlotPrefab;

	[SerializeField]
	private LootInventorySlotView m_Slot;

	[SerializeField]
	private GameObject m_SliderBlock;

	[SerializeField]
	protected Slider m_Slider;

	[SerializeField]
	private TextMeshProUGUI m_SliderText;

	private const string ResultTextFormat = "{0}/{1}";

	public void Initialize()
	{
		m_Header.text = "//- " + UIStrings.Instance.Vendor.ProceedTransaction.Text + " ---/-";
	}

	protected override void BindViewImplementation()
	{
		m_Slot.Bind(base.ViewModel.Slot);
		base.gameObject.SetActive(value: true);
		m_SliderBlock.SetActive(base.ViewModel.MaxValue > 1);
		AddDisposable(m_Slider.OnValueChangedAsObservable().Subscribe(OnSliderValueChanged));
		m_Slider.minValue = 1f;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = m_Slider.maxValue;
		m_SliderText.text = $"{base.ViewModel.MaxValue}/{base.ViewModel.MaxValue}";
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	protected void Deal()
	{
		base.ViewModel.Deal();
	}

	protected virtual void Close()
	{
		base.ViewModel.Close();
	}

	protected void OnSliderValueChanged(float value)
	{
		base.ViewModel.CurrentValue = (int)value;
		SetCounterText();
	}

	private void SetCounterText()
	{
		m_SliderText.text = $"{base.ViewModel.CurrentValue}/{base.ViewModel.MaxValue}";
	}
}
