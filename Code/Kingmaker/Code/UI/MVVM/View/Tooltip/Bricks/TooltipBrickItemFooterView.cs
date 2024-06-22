using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickItemFooterView : TooltipBaseBrickView<TooltipBrickItemFooterVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_LeftLabel;

	[SerializeField]
	protected GameObject m_LeftSide;

	[SerializeField]
	protected TextMeshProUGUI m_RightLabel;

	[SerializeField]
	protected GameObject m_RightSide;

	[SerializeField]
	private GameObject m_LeftEmpty;

	[SerializeField]
	private GameObject m_LeftInfo;

	[SerializeField]
	private Image m_Icon;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_LeftLabel, m_RightLabel);
		}
		if ((bool)m_LeftEmpty || (bool)m_LeftInfo)
		{
			m_LeftLabel.text = base.ViewModel.LeftLine;
			m_LeftInfo.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
			m_LeftInfo.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.LeftAlignment;
			m_LeftEmpty.SetActive(!m_LeftInfo.activeSelf);
			m_RightLabel.text = base.ViewModel.RightLine;
			m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
			m_RightSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.RightAlignment;
			if (base.ViewModel.Icon != null)
			{
				m_Icon.color = Color.white;
			}
		}
		else
		{
			m_LeftLabel.text = base.ViewModel.LeftLine;
			m_LeftSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
			m_LeftSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.LeftAlignment;
			m_RightLabel.text = base.ViewModel.RightLine;
			m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
			m_RightSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.RightAlignment;
		}
		m_Icon.sprite = base.ViewModel.Icon;
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
