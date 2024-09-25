using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public abstract class InsertableLootSlotView : ItemSlotView<InsertableLootSlotVM>
{
	[SerializeField]
	private GameObject m_CanNotInsert;

	public bool CanInsert => base.ViewModel.CanInsert.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CanInsert.Subscribe(delegate(bool value)
		{
			if (m_CanNotInsert != null)
			{
				m_CanNotInsert.gameObject.SetActive(!value);
			}
		}));
	}

	protected override void ClearView()
	{
		base.ClearView();
		if (m_CanNotInsert != null)
		{
			m_CanNotInsert.gameObject.SetActive(value: false);
		}
	}

	protected void OnClick()
	{
		if (base.ViewModel.CanInsert.Value)
		{
			EventBus.RaiseEvent(delegate(INewSlotsHandler h)
			{
				h.HandleTryInsertSlot(base.ViewModel);
			});
		}
	}
}
