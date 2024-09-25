using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public class CharInfoSoulMarkSectorView : CharInfoComponentView<CharInfoSoulMarksSectorVM>
{
	[FormerlySerializedAs("m_SectorLabel")]
	[SerializeField]
	private OwlcatMultiButton m_SectorInfo;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[Header("Progress")]
	[SerializeField]
	private Image m_LevelProgressImage;

	[Header("Slots")]
	[SerializeField]
	private CharInfoAlignmentAbilitySlotPCView[] m_AbilitySlots;

	[SerializeField]
	private float[] m_GlobalProgressThreshold;

	private TooltipHandler m_TooltipHandler;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_Value, m_Level);
		}
		base.BindViewImplementation();
		m_Title.text = UIUtility.GetSoulMarkDirectionText(base.ViewModel.Direction);
		RefreshTooltip();
		BindSlots();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_AbilitySlots.ForEach(delegate(CharInfoAlignmentAbilitySlotPCView s)
		{
			s.Unbind();
		});
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		SetupSector();
		RefreshTooltip();
	}

	private void RefreshTooltip()
	{
		m_TooltipHandler?.Dispose();
		m_TooltipHandler = m_SectorInfo.SetTooltip(base.ViewModel.Tooltip);
		AddDisposable(m_TooltipHandler);
	}

	private void BindSlots()
	{
		int num = Mathf.Min(m_AbilitySlots.Length, base.ViewModel.AbilitySlotVms.Count);
		for (int i = 0; i < num; i++)
		{
			m_AbilitySlots[i].BindSection(base.ViewModel.AbilitySlotVms[i]);
		}
		for (int j = num; j < m_AbilitySlots.Length; j++)
		{
			m_AbilitySlots[j].gameObject.SetActive(value: false);
		}
	}

	private void SetupSector()
	{
		int currentLevel = base.ViewModel.CurrentLevel;
		m_Level.text = UIUtility.ArabicToRoman(Mathf.Min(base.ViewModel.CurrentLevel, base.ViewModel.AbilitySlotVms.Count));
		int num = ((currentLevel + 1 < base.ViewModel.RankThresholds.Count) ? base.ViewModel.RankThresholds[currentLevel + 1] : base.ViewModel.MaxRank);
		m_Value.text = base.ViewModel.CurrentRank + "/" + num;
		if (currentLevel + 1 < base.ViewModel.RankThresholds.Count && currentLevel + 1 < m_GlobalProgressThreshold.Length)
		{
			float num2 = 1f * (float)(base.ViewModel.CurrentRank - base.ViewModel.RankThresholds[currentLevel]) / (float)(base.ViewModel.RankThresholds[currentLevel + 1] - base.ViewModel.RankThresholds[currentLevel]);
			m_LevelProgressImage.fillAmount = num2;
			float globalProgress = Mathf.Lerp(m_GlobalProgressThreshold[currentLevel], m_GlobalProgressThreshold[currentLevel + 1], num2);
			SetGlobalProgress(globalProgress);
		}
		else
		{
			m_LevelProgressImage.fillAmount = 1f;
			SetGlobalProgress(1f);
		}
	}

	private void SetGlobalProgress(float amount)
	{
	}

	public void SetSectorColor(Color sectorColor)
	{
		m_Level.color = sectorColor;
		m_Title.color = sectorColor;
		m_Value.color = sectorColor;
	}

	public List<SimpleConsoleNavigationEntity> GetEntities()
	{
		List<SimpleConsoleNavigationEntity> list = m_AbilitySlots.Select((CharInfoAlignmentAbilitySlotPCView s) => s.NavigationEntity).ToList();
		list.Add(new SimpleConsoleNavigationEntity(m_SectorInfo, base.ViewModel.Tooltip));
		return list;
	}
}
