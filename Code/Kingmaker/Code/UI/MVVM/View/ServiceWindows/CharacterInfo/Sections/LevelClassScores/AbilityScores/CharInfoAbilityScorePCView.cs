using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoAbilityScorePCView : ViewBase<CharInfoStatVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[SerializeField]
	private TextMeshProUGUI m_LongName;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[Header("Diff")]
	[SerializeField]
	private TextMeshProUGUI m_DiffValueLabel;

	[SerializeField]
	private GameObject m_DiffState;

	[Header("Visual")]
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private int m_SecondaryCharSizePercent;

	[SerializeField]
	private Color m_AccentCharColor;

	private bool m_HasPreviewValue;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_ShortName, m_LongName, m_Value, m_DiffValueLabel);
		}
		UISounds.Instance.SetClickSound(m_Selectable, UISounds.ButtonSoundsEnum.NoSound);
		UISounds.Instance.SetHoverSound(m_Selectable, Game.Instance.IsControllerGamepad ? UISounds.ButtonSoundsEnum.PaperComponentSound : UISounds.ButtonSoundsEnum.NoSound);
		AddDisposable(base.ViewModel.Name.Subscribe(delegate
		{
			SetLabels();
		}));
		AddDisposable(base.ViewModel.HasPenalties.CombineLatest(base.ViewModel.HasBonuses, (bool b, bool b1) => new { }).Subscribe(_ =>
		{
			SetBonuses();
		}));
		AddDisposable(base.ViewModel.StatValue.CombineLatest(base.ViewModel.PreviewStatValue, base.ViewModel.Bonus, (int stat, int previewStat, int bonus) => new { stat, previewStat, bonus }).Subscribe(obj =>
		{
			SetValue();
			m_HasPreviewValue = obj.stat != obj.previewStat;
			m_DiffState.Or(null)?.SetActive(m_HasPreviewValue);
			SetBonuses();
			if ((bool)m_DiffValueLabel)
			{
				if (m_HasPreviewValue)
				{
					int num = obj.previewStat - obj.stat;
					m_DiffValueLabel.text = UIUtility.AddSign(num) ?? "";
				}
				else if (obj.bonus != 0)
				{
					m_DiffValueLabel.text = UIUtility.AddSign(obj.bonus) ?? "";
				}
			}
		}));
		SetTooltip();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	private void SetLabels()
	{
		if (m_LongName != null)
		{
			m_LongName.text = base.ViewModel.Name.Value;
		}
		if (m_ShortName != null)
		{
			m_ShortName.text = base.ViewModel.ShortName;
		}
	}

	private void SetValue()
	{
		if (!base.ViewModel.IsValueEnabled.Value)
		{
			if (m_Value != null)
			{
				m_Value.text = "-";
			}
		}
		else if (!(m_Value == null))
		{
			string text = base.ViewModel.StatValue.ToString();
			string text2 = ((text.Length > 1) ? text.Substring(1) : string.Empty);
			m_Value.text = $"<color=#{ColorUtility.ToHtmlStringRGB(m_AccentCharColor)}>{text[0]}</color><size={m_SecondaryCharSizePercent}%>{text2}</size>";
		}
	}

	private void SetBonuses()
	{
		if ((bool)m_Selectable)
		{
			if (m_HasPreviewValue)
			{
				m_Selectable.SetActiveLayer("Preview");
			}
			else if (base.ViewModel.HasBonuses.Value && base.ViewModel.Bonus.Value > 0)
			{
				m_Selectable.SetActiveLayer("Bonus");
			}
			else if (base.ViewModel.HasPenalties.Value && base.ViewModel.Bonus.Value < 0)
			{
				m_Selectable.SetActiveLayer("Penalty");
			}
			else
			{
				m_Selectable.SetActiveLayer("Normal");
			}
		}
	}

	private void SetTooltip()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}
