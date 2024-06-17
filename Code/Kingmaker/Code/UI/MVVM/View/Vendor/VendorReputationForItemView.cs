using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationForItemView : VirtualListElementViewBase<VendorReputationForItemVM>
{
	[FormerlySerializedAs("m_Name")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_ReputationValue;

	[FormerlySerializedAs("Image")]
	[SerializeField]
	private Image m_Image;

	protected override void BindViewImplementation()
	{
		m_Image.sprite = base.ViewModel.TypeIcon;
		if ((bool)m_NameLabel)
		{
			m_NameLabel.text = base.ViewModel.TypeLabel;
		}
		m_ReputationValue.text = base.ViewModel.ReputationCost.ToString();
	}

	protected override void DestroyViewImplementation()
	{
	}
}
