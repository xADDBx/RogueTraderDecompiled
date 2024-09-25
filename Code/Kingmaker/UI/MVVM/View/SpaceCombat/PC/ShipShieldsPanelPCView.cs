using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipShieldsPanelPCView : ViewBase<ShipShieldsPanelVM>
{
	[Header("Ship Doll")]
	[SerializeField]
	private Image m_ShipFill;

	[SerializeField]
	private TextMeshProUGUI m_ShipHealth;

	[Header("Ship Armor")]
	[SerializeField]
	private Image m_ForeArmor;

	[SerializeField]
	private Image m_PortArmor;

	[SerializeField]
	private Image m_StarboardArmor;

	[SerializeField]
	private Image m_AftArmor;

	[Space]
	[SerializeField]
	private FadeAnimator m_ArmorLabels;

	[SerializeField]
	private TextMeshProUGUI m_ForeArmorText;

	[SerializeField]
	private TextMeshProUGUI m_PortArmorText;

	[SerializeField]
	private TextMeshProUGUI m_StarboardArmorText;

	[SerializeField]
	private TextMeshProUGUI m_AftArmorText;

	[Header("Ship Shields")]
	[SerializeField]
	private Image m_ForeShields;

	[SerializeField]
	private Image m_PortShields;

	[SerializeField]
	private Image m_StarboardShields;

	[SerializeField]
	private Image m_AftShields;

	[Space]
	[SerializeField]
	private FadeAnimator m_ShieldsLabels;

	[SerializeField]
	private TextMeshProUGUI m_ForeShieldsText;

	[SerializeField]
	private TextMeshProUGUI m_PortShieldsText;

	[SerializeField]
	private TextMeshProUGUI m_StarboardShieldsText;

	[SerializeField]
	private TextMeshProUGUI m_AftShieldsText;

	[Header("Shields Tween Params")]
	[SerializeField]
	private float m_TweenDuration = 0.5f;

	[SerializeField]
	private int m_TweenLoops = 4;

	private List<Image> m_ArmorImages;

	private List<Image> m_ShieldsImages;

	private readonly Dictionary<StarshipHitLocation, Sequence> m_ShieldsAnimations = new Dictionary<StarshipHitLocation, Sequence>();

	public void Initialize()
	{
		m_ArmorImages = new List<Image> { m_ForeArmor, m_PortArmor, m_StarboardArmor, m_AftArmor };
		m_ShieldsImages = new List<Image> { m_ForeShields, m_PortShields, m_StarboardShields, m_AftShields };
		m_ArmorLabels.Initialize();
		m_ShieldsLabels.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShipHealthRatio.Subscribe(delegate(float value)
		{
			m_ShipFill.fillAmount = value;
		}));
		AddDisposable(base.ViewModel.ShipHealthText.Subscribe(delegate(string value)
		{
			m_ShipHealth.text = value;
		}));
		AddDisposable(base.ViewModel.ShipArmorFore.Subscribe(delegate(int value)
		{
			m_ForeArmorText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipArmorStarboard.Subscribe(delegate(int value)
		{
			m_StarboardArmorText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipArmorPort.Subscribe(delegate(int value)
		{
			m_PortArmorText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipArmorAft.Subscribe(delegate(int value)
		{
			m_AftArmorText.text = value.ToString();
		}));
		foreach (Image armorImage in m_ArmorImages)
		{
			armorImage.alphaHitTestMinimumThreshold = 0.01f;
			AddDisposable(SubscribePointerEvents(armorImage, m_ArmorLabels));
			AddDisposable(armorImage.SetHint(UIStrings.Instance.SpaceCombatTexts.ArmorHint));
		}
		AddDisposable(base.ViewModel.ShipShieldsFore.Subscribe(delegate(int value)
		{
			m_ForeShieldsText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipShieldsStarboard.Subscribe(delegate(int value)
		{
			m_StarboardShieldsText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipShieldsPort.Subscribe(delegate(int value)
		{
			m_PortShieldsText.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.ShipShieldsAft.Subscribe(delegate(int value)
		{
			m_AftShieldsText.text = value.ToString();
		}));
		SubscribeShieldsTween(base.ViewModel.ShipShieldsForeRatio, m_ForeShields, StarshipHitLocation.Fore);
		SubscribeShieldsTween(base.ViewModel.ShipShieldsStarboardRatio, m_StarboardShields, StarshipHitLocation.Starboard);
		SubscribeShieldsTween(base.ViewModel.ShipShieldsPortRatio, m_PortShields, StarshipHitLocation.Port);
		SubscribeShieldsTween(base.ViewModel.ShipShieldsAftRatio, m_AftShields, StarshipHitLocation.Aft);
		foreach (Image shieldsImage in m_ShieldsImages)
		{
			shieldsImage.alphaHitTestMinimumThreshold = 0.01f;
			AddDisposable(SubscribePointerEvents(shieldsImage, m_ShieldsLabels));
			AddDisposable(shieldsImage.SetHint(UIStrings.Instance.SpaceCombatTexts.ShieldsHint));
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_ShieldsAnimations.Values.ForEach(delegate(Sequence sequence)
		{
			sequence.Kill();
		});
		m_ShieldsAnimations.Clear();
	}

	private void SubscribeShieldsTween(ReactiveProperty<float> ratioValue, Image shieldImage, StarshipHitLocation hitLocation)
	{
		AddDisposable(ratioValue.Subscribe(delegate(float value)
		{
			m_ShieldsAnimations.TryGetValue(hitLocation, out var value2);
			value2.Kill();
			float endValue = Mathf.Clamp(value, 0.15f, 1f);
			Sequence sequence = DOTween.Sequence();
			sequence.Append(shieldImage.DOFade(0f, m_TweenDuration).ChangeStartValue(1f).SetLoops(m_TweenLoops, LoopType.Yoyo));
			sequence.Append(shieldImage.DOFade(endValue, m_TweenDuration));
			sequence.SetUpdate(isIndependentUpdate: true).Play();
			m_ShieldsAnimations[hitLocation] = sequence;
		}));
	}

	private IDisposable SubscribePointerEvents(MonoBehaviour target, FadeAnimator animator)
	{
		IDisposable enter = target.OnPointerEnterAsObservable().Subscribe(delegate
		{
			animator.AppearAnimation();
		});
		IDisposable exit = target.OnPointerExitAsObservable().Subscribe(delegate
		{
			animator.DisappearAnimation();
		});
		return Disposable.Create(delegate
		{
			enter?.Dispose();
			exit?.Dispose();
		});
	}
}
