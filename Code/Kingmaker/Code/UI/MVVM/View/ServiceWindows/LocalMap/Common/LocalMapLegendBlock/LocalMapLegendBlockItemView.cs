using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.LocalMapLegendBlock;

public class LocalMapLegendBlockItemView : ViewBase<LocalMapLegendBlockItemVM>
{
	[SerializeField]
	private Image m_ItemImage;

	[SerializeField]
	private TextMeshProUGUI m_ItemLabel;

	protected override void BindViewImplementation()
	{
		m_ItemImage.sprite = base.ViewModel.ItemSprite;
		m_ItemLabel.text = base.ViewModel.ItemLabel;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
