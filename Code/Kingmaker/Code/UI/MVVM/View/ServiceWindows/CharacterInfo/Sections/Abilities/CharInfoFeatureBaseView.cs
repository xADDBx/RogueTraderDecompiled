using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Selectable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public abstract class CharInfoFeatureBaseView : CharInfoFeatureSimpleBaseView
{
	[SerializeField]
	protected OwlcatMultiSelectable m_Button;

	[Header("Texts")]
	[SerializeField]
	protected Image m_TimeIcon;

	[SerializeField]
	protected TextMeshProUGUI m_DisplayName;

	[SerializeField]
	protected TextMeshProUGUI m_Description;

	[SerializeField]
	protected TextMeshProUGUI m_FactDescription;

	[Header("Rank")]
	[SerializeField]
	protected Image m_Rank;

	[SerializeField]
	protected TextMeshProUGUI m_RankText;

	[Header("Tooltip")]
	[SerializeField]
	protected RectTransform m_LeftSideTooltipPlace;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_DisplayName, m_Description);
		}
		base.BindViewImplementation();
		SetupName();
		SetupDescription();
		SetupRank();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
		m_TextHelper.Dispose();
	}

	public void SetupName()
	{
		if (!(m_DisplayName == null))
		{
			m_DisplayName.text = base.ViewModel.DisplayName;
		}
	}

	public void SetupDescription()
	{
		if (m_FactDescription != null)
		{
			m_FactDescription.text = base.ViewModel.FactDescription;
		}
		if (m_Description == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(base.ViewModel.Description) && string.IsNullOrEmpty(base.ViewModel.TimeLeft))
		{
			m_Description.gameObject.SetActive(value: false);
			if (m_TimeIcon != null)
			{
				m_TimeIcon.gameObject.SetActive(value: false);
			}
		}
		else if (!string.IsNullOrEmpty(base.ViewModel.TimeLeft))
		{
			m_Description.gameObject.SetActive(value: true);
			if (m_TimeIcon != null)
			{
				m_TimeIcon.gameObject.SetActive(value: true);
			}
			m_Description.text = base.ViewModel.TimeLeft;
		}
		else if (!string.IsNullOrEmpty(base.ViewModel.Description))
		{
			m_Description.gameObject.SetActive(value: true);
			if (m_TimeIcon != null)
			{
				m_TimeIcon.gameObject.SetActive(value: false);
			}
			m_Description.text = base.ViewModel.Description;
		}
	}

	public void SetupRank()
	{
		if (!(m_Rank == null))
		{
			if (base.ViewModel.Rank.HasValue && base.ViewModel.Rank.Value > 1)
			{
				m_Rank.gameObject.SetActive(value: true);
				m_RankText.gameObject.SetActive(value: true);
				m_RankText.text = Convert.ToString(base.ViewModel.Rank.Value);
			}
			else
			{
				m_Rank.gameObject.SetActive(value: false);
				m_RankText.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void Clear()
	{
		if (m_DisplayName != null)
		{
			m_DisplayName.text = string.Empty;
		}
		if (m_Description != null)
		{
			m_Description.gameObject.SetActive(value: false);
		}
		base.Clear();
	}
}
