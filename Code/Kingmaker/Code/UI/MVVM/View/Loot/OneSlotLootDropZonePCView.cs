using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public class OneSlotLootDropZonePCView : ViewBase<OneSlotLootDropZoneVM>, IItemDropZone
{
	[SerializeField]
	private OwlcatMultiSelectable m_DropZone;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	public bool Interactable => true;

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.HasDropItem.Subscribe(delegate(bool value)
		{
			m_FadeAnimator.PlayAnimation(value);
		}));
		if (m_DropZone != null)
		{
			AddDisposable(base.ViewModel.CanDropItem.Subscribe(delegate(bool value)
			{
				m_DropZone.SetActiveLayer((!value) ? 1 : 0);
			}));
		}
		AddDisposable(this.OnDropAsObservable().Subscribe(OnDrop));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void TryDropItem(ItemSlotVM itemVM)
	{
		base.ViewModel.TryDropItem(itemVM);
	}
}
