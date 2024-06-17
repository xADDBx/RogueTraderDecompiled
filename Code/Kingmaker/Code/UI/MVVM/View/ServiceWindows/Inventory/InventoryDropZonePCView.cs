using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class InventoryDropZonePCView : ViewBase<InventoryDropZoneVM>
{
	[SerializeField]
	private OwlcatMultiSelectable m_DropZone;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
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
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}
