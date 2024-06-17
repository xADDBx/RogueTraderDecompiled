using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;

public class CharInfoSkillPCView : ViewBase<CharInfoStatVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_ValueLabel;

	[Header("Diff")]
	[SerializeField]
	private TextMeshProUGUI m_DiffValueLabel;

	[SerializeField]
	private GameObject m_DiffState;

	[Header("Source stat")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_SourceNameLabel;

	[Header("Visual")]
	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	private bool m_HasPreviewValue;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.SetClickSound(m_Selectable, UISounds.ButtonSoundsEnum.NoSound);
		UISounds.Instance.SetHoverSound(m_Selectable, Game.Instance.IsControllerGamepad ? UISounds.ButtonSoundsEnum.PaperComponentSound : UISounds.ButtonSoundsEnum.NoSound);
		AddDisposable(base.ViewModel.Name.Subscribe(delegate
		{
			SetLabel();
		}));
		AddDisposable(base.ViewModel.HasPenalties.CombineLatest(base.ViewModel.HasBonuses, (bool b, bool b1) => new { }).Subscribe(_ =>
		{
			SetBonuses();
		}));
		AddDisposable(base.ViewModel.StatValue.CombineLatest(base.ViewModel.PreviewStatValue, base.ViewModel.Bonus, (int stat, int previewStat, int bonus) => new { stat, previewStat, bonus }).Subscribe(obj =>
		{
			SetValues(obj.stat, obj.previewStat, obj.bonus);
		}));
		SetTooltip();
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void SetValues(int statValue, int previewValue, int bonus)
	{
		m_ValueLabel.text = statValue.ToString();
		m_HasPreviewValue = statValue != previewValue;
		if ((bool)m_DiffState)
		{
			m_DiffState.SetActive(m_HasPreviewValue);
		}
		if ((bool)m_DiffValueLabel)
		{
			if (m_HasPreviewValue)
			{
				int num = previewValue - statValue;
				m_DiffValueLabel.text = UIUtility.AddSign(num) ?? "";
			}
			else if (bonus != 0)
			{
				m_DiffValueLabel.text = UIUtility.AddSign(bonus) ?? "";
			}
		}
		SetBonuses();
	}

	private void SetLabel()
	{
		m_SourceNameLabel.text = (base.ViewModel.SourceStatType.HasValue ? UIUtilityTexts.GetStatShortName(base.ViewModel.SourceStatType.Value) : string.Empty);
		m_NameLabel.text = base.ViewModel.Name.Value;
	}

	private void SetBonuses()
	{
		if ((bool)m_Selectable)
		{
			if (m_HasPreviewValue)
			{
				m_Selectable.SetActiveLayer("Preview");
			}
			else if (base.ViewModel.HasBonuses.Value)
			{
				m_Selectable.SetActiveLayer("Bonus");
			}
			else if (base.ViewModel.HasPenalties.Value)
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
