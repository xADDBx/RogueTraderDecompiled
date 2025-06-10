using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Glitch;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipHitChanceBlockView : ViewBase<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private CanvasGroup m_AttackStatusesBlock;

	[Header("Hit chance")]
	[SerializeField]
	private CanvasGroup m_HitChanceBlock;

	[SerializeField]
	private TextMeshProUGUI m_HitChance;

	[SerializeField]
	private CanvasGroup m_AbilityBlock;

	[SerializeField]
	private Image m_Ability;

	[SerializeField]
	private Color m_PositiveHintColor;

	[SerializeField]
	private Color m_NegativeHintColor;

	[Header("Damage")]
	[SerializeField]
	private CanvasGroup m_DamageObject;

	[SerializeField]
	private TextMeshProUGUI m_Damage;

	[SerializeField]
	private CanvasGroup m_DamageLine;

	[Header("Burst")]
	[SerializeField]
	private CanvasGroup m_BurstContainer;

	[SerializeField]
	private TextMeshProUGUI m_BurstIndex;

	[SerializeField]
	private Image m_BurstImage;

	[Header("Statuses")]
	[SerializeField]
	private CanvasGroup m_Push;

	[SerializeField]
	private Image m_PushImage;

	[Header("Initial chance")]
	[SerializeField]
	private TextMeshProUGUI m_InitialChance;

	[Header("Glitch")]
	[SerializeField]
	private SpriteGlitchSurfaceOvertip m_Glitch;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private ReactiveProperty<string> m_HitChanceHint = new ReactiveProperty<string>();

	private ReactiveProperty<string> m_DamageHint = new ReactiveProperty<string>();

	private ReactiveProperty<string> m_BurstCountHint = new ReactiveProperty<string>();

	private StringBuilder m_StringBuilder = new StringBuilder();

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility;

	public void Initialize(ReactiveProperty<UnitOvertipVisibility> visibilityProperty)
	{
		m_Visibility = visibilityProperty;
	}

	protected override void BindViewImplementation()
	{
		m_HitChanceBlock.alpha = 0f;
		m_AbilityBlock.alpha = 0f;
		AddDisposable(base.ViewModel.IsVisibleTrigger.CombineLatest(base.ViewModel.HasHit, base.ViewModel.HitAlways, base.ViewModel.UnitState.HoverSelfTargetAbility, base.ViewModel.UnitState.Ability, (bool isVisible, bool hasHit, bool hitAlways, bool hoverSelf, AbilityData ability) => new { isVisible, hasHit, hitAlways, hoverSelf, ability }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			bool flag2 = (value.isVisible && value.hasHit) || value.hoverSelf;
			m_FadeAnimator.PlayAnimation(flag2);
			m_DamageObject.alpha = ((flag2 && (base.ViewModel.MinDamage.Value > 0 || base.ViewModel.MaxDamage.Value > 0)) ? 1 : 0);
			UpdateIconAndGlitchVisual(flag2);
		}));
		if ((bool)m_DamageLine)
		{
			AddDisposable(m_Visibility.Subscribe(delegate(UnitOvertipVisibility visibility)
			{
				m_DamageLine.alpha = ((visibility == UnitOvertipVisibility.Maximized) ? 1 : 0);
			}));
		}
		AddDisposable(base.ViewModel.HitChance.Subscribe(delegate(float value)
		{
			m_HitChance.text = Mathf.Round(value).ToString();
		}));
		AddDisposable(base.ViewModel.HitChance.CombineLatest(base.ViewModel.InitialHitChance, base.ViewModel.DodgeChance, base.ViewModel.ParryChance, base.ViewModel.CoverChance, base.ViewModel.BlockChance, (float hitChance, float initHitChance, float dodge, float parry, float cover, float block) => new { hitChance, initHitChance, dodge, parry, cover, block }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_HitChanceHint.Value = GetHitChanceHint(value.hitChance, value.initHitChance, value.dodge, value.parry, value.cover, value.block);
		}));
		AddDisposable(m_HitChance.SetHint(m_HitChanceHint));
		AddDisposable(m_Ability.SetHint(m_HitChanceHint));
		if ((bool)m_Push)
		{
			AddDisposable(base.ViewModel.CanPush.CombineLatest(base.ViewModel.IsVisibleTrigger, (bool can, bool visible) => can && visible).Subscribe(delegate(bool value)
			{
				m_Push.alpha = (value ? 1 : 0);
				m_Push.blocksRaycasts = value;
			}));
			AddDisposable(m_PushImage.SetHint(UIStrings.Instance.Tooltips.PossibleToPush.Text, null, m_PushImage.color));
		}
		if (m_InitialChance != null)
		{
			AddDisposable(base.ViewModel.InitialHitChance.Subscribe(delegate(float value)
			{
				m_InitialChance.text = value.ToString();
			}));
		}
		if (m_BurstIndex != null)
		{
			AddDisposable(base.ViewModel.BurstIndex.CombineLatest(m_Visibility, (int burst, UnitOvertipVisibility visibility) => new { burst, visibility }).Subscribe(value =>
			{
				UnitOvertipVisibility visibility2 = value.visibility;
				bool flag = visibility2 == UnitOvertipVisibility.Full || visibility2 == UnitOvertipVisibility.Maximized;
				m_BurstIndex.text = ((value.burst > 1 && flag) ? $"×{value.burst}" : string.Empty);
				m_BurstContainer.alpha = (flag ? Mathf.Clamp01(value.burst - 1) : 0f);
				m_BurstContainer.blocksRaycasts = value.burst > 1 && flag;
				m_BurstCountHint.Value = $"{value.burst} {UIStrings.Instance.Tooltips.BurstCount.Text}";
			}));
			AddDisposable(m_BurstImage.SetHint(m_BurstCountHint, null, m_BurstIndex.color));
		}
		if (m_Damage != null)
		{
			AddDisposable(base.ViewModel.MinDamage.CombineLatest(base.ViewModel.MaxDamage, (int min, int max) => new { min, max }).ObserveLastValueOnLateUpdate().Subscribe(value =>
			{
				StringBuilder stringBuilder = new StringBuilder($"{value.min}–{value.max}");
				m_Damage.text = stringBuilder.ToString();
				stringBuilder.Append(base.ViewModel.CanDie ? (" " + UIStrings.Instance.Tooltips.Damage.Text + "\n" + UIStrings.Instance.Tooltips.PossibleToKill.Text) : (" " + UIStrings.Instance.Tooltips.Damage.Text));
				m_DamageHint.Value = stringBuilder.ToString();
			}));
			AddDisposable(m_Damage.SetHint(m_DamageHint, null, m_Damage.color));
		}
	}

	private void UpdateIconAndGlitchVisual(bool state)
	{
		Sprite sprite = base.ViewModel.UnitState.HoverAbilityIcon.Or(null) ?? base.ViewModel.UnitState.Ability.Value?.Icon;
		if (state && sprite != null)
		{
			m_Ability.sprite = sprite;
		}
		float value = base.ViewModel.HitChance.Value;
		bool flag = value > 0f && value < 100f;
		bool flag2 = base.ViewModel.HitAlways.Value && !flag;
		bool flag3 = state && !flag2 && !base.ViewModel.UnitState.HoverSelfTargetAbility.Value;
		m_HitChanceBlock.alpha = (flag3 ? 1 : 0);
		m_HitChanceBlock.blocksRaycasts = flag3;
		bool flag4 = state && (flag2 || base.ViewModel.UnitState.HoverSelfTargetAbility.Value) && sprite != null;
		m_AbilityBlock.alpha = (flag4 ? 1 : 0);
		m_AbilityBlock.blocksRaycasts = flag4;
		if (m_Glitch != null)
		{
			if (state && base.ViewModel.HitChance.Value >= 0f)
			{
				m_Glitch.SetIntensivity(100f - base.ViewModel.HitChance.Value);
			}
			else
			{
				m_Glitch.SetLowGlitch();
			}
		}
	}

	private string GetHitChanceHint(float hitChance, float initHitChance, float dodge, float parry, float cover, float block)
	{
		m_StringBuilder.Clear();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		string text = ColorUtility.ToHtmlStringRGB(m_PositiveHintColor);
		string text2 = ColorUtility.ToHtmlStringRGB(m_NegativeHintColor);
		m_StringBuilder.AppendLine("<color=#" + text + ">" + UIUtilityTexts.GetPercentString(hitChance) + " " + tooltips.TotalHitChance.Text + "</color>");
		m_StringBuilder.AppendLine("<color=#" + text + "><separator></color>");
		m_StringBuilder.AppendLine("<color=#" + text + ">" + UIUtilityTexts.GetPercentString(initHitChance) + " " + tooltips.InitialHitChance.Text + "</color>");
		if (dodge > 0f)
		{
			m_StringBuilder.AppendLine("<color=#" + text2 + ">" + UIUtilityTexts.GetPercentString(dodge) + " " + tooltips.DodgeAvoidance.Text + "</color>");
		}
		if (parry > 0f)
		{
			m_StringBuilder.AppendLine("<color=#" + text2 + ">" + UIUtilityTexts.GetPercentString(parry) + " " + tooltips.ParryAvoidance.Text + "</color>");
		}
		if (cover > 0f)
		{
			m_StringBuilder.AppendLine("<color=#" + text2 + ">" + UIUtilityTexts.GetPercentString(cover) + " " + tooltips.CoverAvoidance.Text + "</color>");
		}
		if (block > 0f)
		{
			m_StringBuilder.AppendLine("<color=#" + text2 + ">" + UIUtilityTexts.GetPercentString(block) + " " + tooltips.BlockAvoidance.Text + "</color>");
		}
		return m_StringBuilder.ToString();
	}

	protected override void DestroyViewImplementation()
	{
	}
}
