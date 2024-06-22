using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class ShipPCView : ViewBase<ShipVM>
{
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI m_ShipName;

	[SerializeField]
	private TextMeshProUGUI m_FrontArmor;

	[SerializeField]
	private TextMeshProUGUI m_PortArmor;

	[SerializeField]
	private TextMeshProUGUI m_StarboardArmor;

	[SerializeField]
	private TextMeshProUGUI m_RearArmor;

	[SerializeField]
	private TextMeshProUGUI m_FrontShield;

	[SerializeField]
	private TextMeshProUGUI m_PortShield;

	[SerializeField]
	private TextMeshProUGUI m_StarboardShield;

	[SerializeField]
	private TextMeshProUGUI m_RearShield;

	[SerializeField]
	private TextMeshProUGUI m_Experience;

	[SerializeField]
	private TextMeshProUGUI m_ExperienceText;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[SerializeField]
	private TextMeshProUGUI m_LevelText;

	[SerializeField]
	private Image m_ExpBar;

	[SerializeField]
	protected OwlcatMultiButton m_ProwRamDamageBlock;

	[SerializeField]
	public TextMeshProUGUI ProwRamDamageBonus;

	[SerializeField]
	protected OwlcatMultiButton m_ProwRamSelfDamageReduceBlock;

	[SerializeField]
	public TextMeshProUGUI ProwRamSelfDamageReduceBonus;

	[SerializeField]
	private CanvasGroup m_ProwMark;

	[SerializeField]
	private CanvasGroup m_ShieldsMark;

	[SerializeField]
	private CanvasGroup m_HullMark;

	[SerializeField]
	protected OwlcatMultiButton[] m_Shields;

	[SerializeField]
	protected OwlcatMultiButton[] m_Hulls;

	[SerializeField]
	protected MonoBehaviour TooltipPlace;

	protected TooltipTemplateGlossary ShieldsTooltip;

	protected TooltipTemplateSimple HullTooltip;

	protected TooltipTemplateSimple ProwRamDamageTooltip;

	protected TooltipTemplateSimple ProwRamSelfDamageReduceTooltip;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		m_LevelText.text = UIStrings.Instance.CharacterSheet.LEVEL;
		m_ExperienceText.text = UIStrings.Instance.ShipCustomization.XP;
		AddDisposable(base.ViewModel.ShipName.Subscribe(delegate
		{
			UpdateStats();
		}));
		AddDisposable(base.ViewModel.ShipArmorFront.Subscribe(delegate
		{
			UpdateArmor();
		}));
		AddDisposable(base.ViewModel.ShipArmorLeft.Subscribe(delegate
		{
			UpdateArmor();
		}));
		AddDisposable(base.ViewModel.ShipArmorRight.Subscribe(delegate
		{
			UpdateArmor();
		}));
		AddDisposable(base.ViewModel.ShipArmorRear.Subscribe(delegate
		{
			UpdateArmor();
		}));
		AddDisposable(base.ViewModel.ShipMoraleValue.Subscribe(delegate
		{
			UpdateStats();
		}));
		AddDisposable(base.ViewModel.ShipFrontShield.Subscribe(delegate
		{
			UpdateShields();
		}));
		AddDisposable(base.ViewModel.ShipLeftShield.Subscribe(delegate
		{
			UpdateShields();
		}));
		AddDisposable(base.ViewModel.ShipRightShield.Subscribe(delegate
		{
			UpdateShields();
		}));
		AddDisposable(base.ViewModel.ShipRearShield.Subscribe(delegate
		{
			UpdateShields();
		}));
		AddDisposable(base.ViewModel.ShipExperience.Subscribe(delegate
		{
			UpdateStats();
		}));
		AddDisposable(base.ViewModel.ShipLvl.Subscribe(delegate
		{
			UpdateStats();
		}));
		AddDisposable(base.ViewModel.ProwRamDamageBonus.Subscribe(delegate
		{
			UpdateProwRam();
		}));
		AddDisposable(base.ViewModel.ProwRamSelfDamageReduceBonus.Subscribe(delegate
		{
			UpdateProwRam();
		}));
		ShieldsTooltip = new TooltipTemplateGlossary("SpeedSpace");
		HullTooltip = new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.ArmorPlating, UIStrings.Instance.ShipCustomization.ArmorPlatingDescription);
		string header = UIStrings.Instance.ShipCustomization.RamDamageBonus.Text + " (" + UIStrings.Instance.ShipCustomization.Ram.Text + ")";
		ProwRamDamageTooltip = new TooltipTemplateSimple(header, UIStrings.Instance.ShipCustomization.RamDamageBonusDescription);
		string header2 = UIStrings.Instance.ShipCustomization.RamDamageReduction.Text + " (" + UIStrings.Instance.ShipCustomization.Ram.Text + ")";
		ProwRamSelfDamageReduceTooltip = new TooltipTemplateSimple(header2, UIStrings.Instance.ShipCustomization.RamDamageReductionDescription.Text);
		AddDisposable(m_ProwRamDamageBlock.SetTooltip(ProwRamDamageTooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		AddDisposable(m_ProwRamSelfDamageReduceBlock.SetTooltip(ProwRamSelfDamageReduceTooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(delegate
		{
			ShowPart();
		}));
		OwlcatMultiButton[] shields = m_Shields;
		foreach (OwlcatMultiButton component in shields)
		{
			AddDisposable(component.SetGlossaryTooltip("VoidshipShields", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		}
		shields = m_Hulls;
		foreach (OwlcatMultiButton component2 in shields)
		{
			AddDisposable(component2.SetTooltip(HullTooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		}
	}

	private void UpdateStats()
	{
		m_ShipName.text = base.ViewModel.ShipName.Value;
		m_Experience.text = base.ViewModel.ShipExperience.Value;
		m_ExpBar.fillAmount = base.ViewModel.ShipExperienceBar.Value;
		m_Level.text = base.ViewModel.ShipLvl.ToString();
	}

	private void UpdateProwRam()
	{
		ProwRamDamageBonus.text = base.ViewModel.ProwRamDamageBonus.ToString();
		ProwRamSelfDamageReduceBonus.text = base.ViewModel.ProwRamSelfDamageReduceBonus.ToString();
		Blink(m_ProwMark);
	}

	private void UpdateShields()
	{
		m_FrontShield.text = base.ViewModel.ShipFrontShield.Value.ToString();
		m_PortShield.text = base.ViewModel.ShipLeftShield.Value.ToString();
		m_StarboardShield.text = base.ViewModel.ShipRightShield.Value.ToString();
		m_RearShield.text = base.ViewModel.ShipRearShield.Value.ToString();
		Blink(m_ShieldsMark);
	}

	private void UpdateArmor()
	{
		m_FrontArmor.text = base.ViewModel.ShipArmorFront.Value.ToString();
		m_PortArmor.text = base.ViewModel.ShipArmorRight.Value.ToString();
		m_StarboardArmor.text = base.ViewModel.ShipArmorLeft.Value.ToString();
		m_RearArmor.text = base.ViewModel.ShipArmorRear.Value.ToString();
		Blink(m_HullMark);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SelectShip()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		UIAccess.SelectionManager.SelectUnit(playerShip.View);
		Game.Instance.CameraController?.Follower?.ScrollTo(playerShip);
	}

	public void Blink(CanvasGroup canvasGroup)
	{
		UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
		canvasGroup.gameObject.SetActive(value: true);
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
	}

	private void OnShow()
	{
		base.gameObject.SetActive(base.ViewModel.ShouldShow.Value);
	}

	private void ShowPart()
	{
		OnShow();
	}
}
