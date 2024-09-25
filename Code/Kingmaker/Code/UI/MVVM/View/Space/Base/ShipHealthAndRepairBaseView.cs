using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Space.Base;

public class ShipHealthAndRepairBaseView : ViewBase<ShipHealthAndRepairVM>
{
	[Header("Main Part")]
	[SerializeField]
	private TextMeshProUGUI m_ScrapWeHave;

	[SerializeField]
	private TextMeshProUGUI m_VoidshipHealthText;

	[SerializeField]
	private RectTransform m_HPBarMaxHealth;

	[SerializeField]
	private RectTransform m_HPBarHealth;

	[SerializeField]
	private RectTransform m_HPBarHealthAfterRepair;

	[SerializeField]
	private Image m_ShipIcon;

	[Header("Button Repair")]
	[SerializeField]
	private FadeAnimator m_ButtonRepairAnimator;

	[SerializeField]
	private GameObject m_NeedRepairBlock;

	[SerializeField]
	private TextMeshProUGUI m_CostRepair;

	[SerializeField]
	private TextMeshProUGUI m_RepairHullText;

	[SerializeField]
	private GameObject m_FullHpBlock;

	[SerializeField]
	private TextMeshProUGUI m_FullHpText;

	[SerializeField]
	private GameObject m_NeedMoreMoneyBlock;

	[SerializeField]
	private TextMeshProUGUI m_NeedMoreMoneyText;

	[SerializeField]
	private TextMeshProUGUI m_HowMuchMoneyWeNeed;

	[Header("Parts Labels")]
	public bool HasPartsLabels;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_EngineLabel;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_RamLabel;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_ShieldsLabel;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_RepairLabel;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_DamageReductionLabel;

	[ConditionalShow("HasPartsLabels")]
	[SerializeField]
	private TextMeshProUGUI m_DamageBonusLabel;

	[SerializeField]
	private CanvasGroup[] m_HealthMark;

	private float m_MaxWidthHP;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(base.gameObject.SetActive));
		m_ButtonRepairAnimator.gameObject.SetActive(value: false);
		m_RepairHullText.text = UIStrings.Instance.SystemMap.RepairHullSimple;
		m_MaxWidthHP = (m_HPBarMaxHealth ? m_HPBarMaxHealth.sizeDelta.x : m_HPBarHealth.sizeDelta.x);
		m_NeedMoreMoneyText.text = UIStrings.Instance.SystemMap.YouNeedMoreScrap;
		if (base.ViewModel.PlayerShipSprite != null && m_ShipIcon != null)
		{
			m_ShipIcon.sprite = base.ViewModel.PlayerShipSprite;
		}
		if (HasPartsLabels)
		{
			m_EngineLabel.text = UIStrings.Instance.ShipCustomization.Engine;
			m_RamLabel.text = UIStrings.Instance.ShipCustomization.Ram;
			m_ShieldsLabel.text = UIStrings.Instance.ShipCustomization.Shields;
			m_RepairLabel.text = UIStrings.Instance.ShipCustomization.Repair;
			m_DamageReductionLabel.text = UIStrings.Instance.ShipCustomization.RamDamageReduction;
			m_DamageBonusLabel.text = UIStrings.Instance.ShipCustomization.RamDamageBonus;
		}
		AddDisposable(base.ViewModel.ScrapWeHave.Subscribe(SetScrapWeHave));
		AddDisposable(base.ViewModel.MaxShipHealth.CombineLatest(base.ViewModel.CurrentShipHealth, (int msh, int csh) => new { msh, csh }).Subscribe(_ =>
		{
			SetVoidshipHealth();
		}));
		AddDisposable(base.ViewModel.ScrapNeedForRepair.Subscribe(SetScrapForRepair));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetScrapWeHave(int scrap)
	{
		m_ScrapWeHave.text = $"<sprite=0>{scrap}";
	}

	private void SetVoidshipHealth()
	{
		if (base.ViewModel.MaxShipHealth.Value > 0)
		{
			float num = (float)base.ViewModel.CurrentShipHealth.Value / (float)base.ViewModel.MaxShipHealth.Value;
			m_VoidshipHealthText.text = $"{base.ViewModel.CurrentShipHealth.Value}/{base.ViewModel.MaxShipHealth.Value}";
			m_HPBarHealth.DOSizeDelta(new Vector2(m_MaxWidthHP * num, m_HPBarHealth.sizeDelta.y), 0.5f).OnPlay(delegate
			{
				UISounds.Instance.Sounds.ShipHealthAndRepair.HealthLineMove.Play();
			}).SetUpdate(isIndependentUpdate: true);
			m_HPBarHealthAfterRepair.DOSizeDelta(new Vector2(m_MaxWidthHP * num, m_HPBarHealthAfterRepair.sizeDelta.y), 0.5f).SetUpdate(isIndependentUpdate: true);
			CanvasGroup[] healthMark = m_HealthMark;
			foreach (CanvasGroup canvasGroup in healthMark)
			{
				Blink(canvasGroup);
			}
		}
	}

	private void SetScrapForRepair(int scrap)
	{
		string text = ((scrap == 0) ? "" : "-");
		m_CostRepair.text = text + $"{scrap}<sprite=0 color=#4A310A>";
	}

	protected void SetButtonRepairHover(bool state, bool forceClose = false, bool needToFillBack = true, bool afterRepair = false, GameObject pcGameDialogObject = null)
	{
		if (!base.ViewModel.CanRepair.Value)
		{
			m_NeedRepairBlock.SetActive(value: false);
			m_NeedMoreMoneyBlock.SetActive(value: false);
			m_FullHpBlock.SetActive(value: false);
			m_ButtonRepairAnimator.DisappearAnimation();
			return;
		}
		if (state)
		{
			int value = base.ViewModel.CurrentShipHealth.Value;
			int value2 = base.ViewModel.MaxShipHealth.Value;
			int value3 = base.ViewModel.ScrapWeHave.Value;
			if (value == value2)
			{
				m_FullHpBlock.SetActive(value: true);
				m_FullHpText.text = UIStrings.Instance.SystemMap.ShipHpIsFull;
				m_NeedRepairBlock.SetActive(value: false);
				m_NeedMoreMoneyBlock.SetActive(value: false);
			}
			else if (value3 == 0)
			{
				m_FullHpBlock.SetActive(value: false);
				m_NeedRepairBlock.SetActive(value: false);
				m_NeedMoreMoneyBlock.SetActive(value: true);
				m_HowMuchMoneyWeNeed.text = $"{Game.Instance.Player.Scrap.ScrapNeededForFullRepair - value3}";
			}
			else
			{
				m_FullHpBlock.SetActive(value: false);
				m_NeedRepairBlock.SetActive(value: true);
				SetScrapForRepair((base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? base.ViewModel.ScrapWeHave.Value : base.ViewModel.ScrapNeedForRepair.Value);
				m_NeedMoreMoneyBlock.SetActive(value: false);
				float num = (float)value / (float)value2 + (float)value3 / (float)value2;
				float num2 = (float)base.ViewModel.ScrapNeedForRepair.Value / (float)value2;
				float shipRepairCostModifiers = Game.Instance.Player.Scrap.ShipRepairCostModifiers;
				float num3 = SpacecombatDifficultyHelper.RepairCostMod();
				float num4 = ((shipRepairCostModifiers != 1f) ? (1f / shipRepairCostModifiers) : 1f);
				float num5 = ((num3 != 1f) ? (1f / num3) : 1f);
				float x = ((value3 < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? (m_MaxWidthHP * num) : (num2 * m_MaxWidthHP * num4 * num5 + m_MaxWidthHP * ((float)value / (float)value2)));
				float num6 = Mathf.Min(value + value3, value2);
				m_VoidshipHealthText.text = $"<color=#73BE53>{num6}</color>/{value2}";
				m_HPBarHealthAfterRepair.DOSizeDelta(new Vector2(x, m_HPBarHealthAfterRepair.sizeDelta.y), 0.5f).OnPlay(delegate
				{
					UISounds.Instance.Sounds.ShipHealthAndRepair.HowManyHealthWillRepairLineMove.Play();
				}).SetUpdate(isIndependentUpdate: true);
			}
			m_ButtonRepairAnimator.AppearAnimation();
			return;
		}
		if (pcGameDialogObject != null)
		{
			if (pcGameDialogObject.activeSelf && !forceClose)
			{
				return;
			}
		}
		else if (!forceClose)
		{
			return;
		}
		m_VoidshipHealthText.text = $"{base.ViewModel.CurrentShipHealth.Value}/{base.ViewModel.MaxShipHealth.Value}";
		if (afterRepair)
		{
			m_FullHpBlock.SetActive(value: true);
			m_FullHpText.text = UIStrings.Instance.SystemMap.ShipIsRepaired;
			UISounds.Instance.Sounds.ShipHealthAndRepair.ShipRepaired.Play();
			m_NeedRepairBlock.SetActive(value: false);
			m_NeedMoreMoneyBlock.SetActive(value: false);
			DelayedInvoker.InvokeInTime(delegate
			{
				m_ButtonRepairAnimator.DisappearAnimation();
			}, 2f);
		}
		else
		{
			m_ButtonRepairAnimator.DisappearAnimation();
		}
		if (!needToFillBack)
		{
			return;
		}
		float num7 = (float)base.ViewModel.CurrentShipHealth.Value / (float)base.ViewModel.MaxShipHealth.Value;
		float num8 = m_MaxWidthHP * num7;
		if (m_HPBarHealthAfterRepair.sizeDelta.x != num8)
		{
			m_HPBarHealthAfterRepair.DOSizeDelta(new Vector2(num8, m_HPBarHealthAfterRepair.sizeDelta.y), 0.5f).OnPlay(delegate
			{
				UISounds.Instance.Sounds.ShipHealthAndRepair.HowManyHealthWillRepairLineMove.Play();
			}).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void Blink(CanvasGroup canvasGroup)
	{
		UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
		canvasGroup.gameObject.SetActive(value: true);
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
	}
}
