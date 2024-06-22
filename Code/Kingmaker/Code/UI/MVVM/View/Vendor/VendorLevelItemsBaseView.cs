using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorLevelItemsBaseView : ViewBase<VendorLevelItemsVM>, IWidgetView
{
	[SerializeField]
	private VendorSlotView m_VendorSlotView;

	[SerializeField]
	private OwlcatMultiButton m_LockButton;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	protected TextMeshProUGUI m_LevelValue;

	[SerializeField]
	protected Image m_FillAmount;

	[SerializeField]
	protected GameObject m_LastItemMark;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		m_LevelValue.text = base.ViewModel.ReputationLevelVM.ReputationLevel.ToString();
		m_LockButton.SetActiveLayer(base.ViewModel.ReputationLevelVM.Locked ? "Locked" : "Unlocked");
		m_FillAmount.fillAmount = (float)base.ViewModel.ReputationLevelVM.Difference / (float)base.ViewModel.ReputationLevelVM.Delta;
		m_LastItemMark.Or(null)?.SetActive(base.ViewModel.IsLastList);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.VendorSlots, m_VendorSlotView);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as VendorLevelItemsVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is VendorLevelItemsVM;
	}
}
