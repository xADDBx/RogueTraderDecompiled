using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipTargetDefensesView : ViewBase<OvertipHitChanceBlockVM>
{
	private enum DefenseType
	{
		Dodge,
		Parry,
		Cover
	}

	[Header("Dodge")]
	[SerializeField]
	private CanvasGroup m_Dodge;

	[SerializeField]
	private TextMeshProUGUI m_DodgeChance;

	[SerializeField]
	private Image m_DodgeHintPlace;

	private ReactiveProperty<string> m_DodgeHint = new ReactiveProperty<string>();

	[Header("Parry")]
	[SerializeField]
	private CanvasGroup m_Parry;

	[SerializeField]
	private TextMeshProUGUI m_ParryChance;

	[SerializeField]
	private Image m_ParryHintPlace;

	private ReactiveProperty<string> m_ParryHint = new ReactiveProperty<string>();

	[Header("Cover")]
	[SerializeField]
	private CanvasGroup m_Cover;

	[SerializeField]
	private TextMeshProUGUI m_CoverChance;

	[SerializeField]
	private Image m_CoverHintPlace;

	private ReactiveProperty<string> m_CoverHint = new ReactiveProperty<string>();

	private StringBuilder m_StringBuilder = new StringBuilder();

	private List<CanvasGroup> m_CanvasGroups;

	private List<TextMeshProUGUI> m_Labels;

	private List<ReactiveProperty<string>> m_Hints;

	private List<string> m_Strings;

	private List<Image> m_HintPlaces;

	protected override void BindViewImplementation()
	{
		m_CanvasGroups = new List<CanvasGroup> { m_Dodge, m_Parry, m_Cover };
		m_Labels = new List<TextMeshProUGUI> { m_DodgeChance, m_ParryChance, m_CoverChance };
		m_Hints = new List<ReactiveProperty<string>> { m_DodgeHint, m_ParryHint, m_CoverHint };
		UICombatTexts combatTexts = UIStrings.Instance.CombatTexts;
		m_Strings = new List<string> { combatTexts.Dodge, combatTexts.Parried, combatTexts.Cover };
		m_HintPlaces = new List<Image> { m_DodgeHintPlace, m_ParryHintPlace, m_CoverHintPlace };
		AddDisposable(base.ViewModel.DodgeChance.CombineLatest(base.ViewModel.IsVisibleTrigger, (float chance, bool visible) => new { chance, visible }).Subscribe(value =>
		{
			SetValues(value.chance, value.visible, DefenseType.Dodge);
		}));
		AddDisposable(base.ViewModel.ParryChance.CombineLatest(base.ViewModel.IsVisibleTrigger, (float chance, bool visible) => new { chance, visible }).Subscribe(value =>
		{
			SetValues(value.chance, value.visible, DefenseType.Parry);
		}));
		AddDisposable(base.ViewModel.CoverChance.CombineLatest(base.ViewModel.IsVisibleTrigger, (float chance, bool visible) => new { chance, visible }).Subscribe(value =>
		{
			SetValues(value.chance, value.visible, DefenseType.Cover);
		}));
		for (int i = 0; i < Enum.GetNames(typeof(DefenseType)).Length; i++)
		{
			AddDisposable(m_HintPlaces[i].SetHint(m_Hints[i], null, m_Labels[i].color));
		}
	}

	private void SetValues(float chance, bool visible, DefenseType type)
	{
		m_CanvasGroups[(int)type].alpha = Mathf.Clamp01(chance);
		m_CanvasGroups[(int)type].blocksRaycasts = visible && chance > 0f;
		m_StringBuilder.Append(UIUtilityTexts.GetPercentString(chance));
		m_Labels[(int)type].text = m_StringBuilder.ToString();
		m_StringBuilder.Append(" " + m_Strings[(int)type]);
		m_Hints[(int)type].Value = m_StringBuilder.ToString();
		m_StringBuilder.Clear();
	}

	protected override void DestroyViewImplementation()
	{
	}
}
