using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickFeatureView : TooltipBaseBrickView<TooltipBrickFeatureVM>
{
	[Serializable]
	public class MutableFeatureElements
	{
		[SerializeField]
		internal TextMeshProUGUI Label;
	}

	[FormerlySerializedAs("m_FeatureElementsList")]
	[SerializeField]
	protected List<MutableFeatureElements> m_MutableElementsList = new List<MutableFeatureElements>();

	[SerializeField]
	protected GameObject m_Background;

	[SerializeField]
	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	protected GameObject m_IconBlock;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(((IEnumerable<MutableFeatureElements>)m_MutableElementsList).Select((Func<MutableFeatureElements, TMP_Text>)((MutableFeatureElements e) => e.Label)).ToArray());
		base.BindViewImplementation();
		int num = (int)base.ViewModel.Type;
		if (num >= m_MutableElementsList.Count)
		{
			num = 0;
		}
		for (int i = 0; i < m_MutableElementsList.Count; i++)
		{
			m_MutableElementsList[i].Label.gameObject.SetActive(i == num);
		}
		m_MutableElementsList.ElementAt(num).Label.text = base.ViewModel.Name;
		m_IconBlock.SetActive((bool)base.ViewModel.Icon || base.ViewModel.Acronym != null);
		if ((bool)base.ViewModel.Icon || base.ViewModel.Acronym != null)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.color = base.ViewModel.IconColor;
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
		}
		else
		{
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
		}
		m_Acronym.text = base.ViewModel.Acronym;
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f),
				new Vector2(0f, 0.5f)
			}
		}));
		if ((bool)m_Background)
		{
			m_Background.SetActive(base.ViewModel.AvailableBackground);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
