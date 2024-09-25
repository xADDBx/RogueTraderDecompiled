using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipComponentItemSlotBaseView : VirtualListElementViewBase<ShipComponentItemSlotVM>, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected TMP_Text m_Text;

	[SerializeField]
	protected Image m_Icon;

	public ShipComponentItemSlotVM GetVM => base.ViewModel;

	protected override void BindViewImplementation()
	{
		SetupName();
		SetupIcon();
	}

	public void SetupName()
	{
		if (!(m_Text == null))
		{
			m_Text.text = base.ViewModel.DisplayName;
		}
	}

	public void SetupIcon()
	{
		m_Icon.color = Color.white;
		m_Icon.sprite = base.ViewModel.Icon;
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected void OnClick()
	{
		EventBus.RaiseEvent(delegate(ISelectingWindowFocusHandler h)
		{
			h.Focus(base.ViewModel);
		});
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}
