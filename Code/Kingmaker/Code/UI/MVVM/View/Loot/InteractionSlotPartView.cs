using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public class InteractionSlotPartView : ViewBase<InteractionSlotPartVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private GameObject m_DescriptionBlock;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	protected LootSlotView m_SlotView;

	[SerializeField]
	private OneSlotLootDropZonePCView m_OneSlotLootDropZonePCView;

	public void Initialize()
	{
		Hide();
	}

	protected override void BindViewImplementation()
	{
		Show();
		SetLabels();
		BindSlot();
		if (m_OneSlotLootDropZonePCView != null)
		{
			AddDisposable(base.ViewModel.DropZoneVM.Subscribe(m_OneSlotLootDropZonePCView.Bind));
		}
	}

	private void SetLabels()
	{
		m_DescriptionBlock.SetActive(base.ViewModel.Description != string.Empty);
		m_Title.text = base.ViewModel.Name;
		m_Description.text = base.ViewModel.Description;
	}

	private void BindSlot()
	{
		AddDisposable(base.ViewModel.ItemSlot.Subscribe(m_SlotView.Bind));
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
