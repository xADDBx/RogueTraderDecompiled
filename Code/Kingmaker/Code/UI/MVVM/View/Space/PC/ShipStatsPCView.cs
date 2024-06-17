using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class ShipStatsPCView : ViewBase<ShipStatsVM>
{
	[Header("Ship Stats")]
	[SerializeField]
	protected OwlcatMultiButton m_SpeedBlock;

	[SerializeField]
	private TextMeshProUGUI m_Speed;

	[SerializeField]
	private TextMeshProUGUI m_SpeedText;

	[SerializeField]
	protected OwlcatMultiButton m_InertiaBlock;

	[SerializeField]
	private TextMeshProUGUI m_Inertia;

	[SerializeField]
	private TextMeshProUGUI m_InertiaText;

	[SerializeField]
	protected MonoBehaviour TooltipPlace;

	[SerializeField]
	[UsedImplicitly]
	private CanvasGroup m_AnimatedMark;

	protected TooltipTemplateGlossary m_SpeedTooltip;

	protected TooltipTemplateGlossary m_InertiaTooltip;

	protected override void BindViewImplementation()
	{
		m_SpeedText.text = UIStrings.Instance.CharacterSheet.Speed;
		m_InertiaText.text = UIStrings.Instance.ShipCustomization.Inertia;
		AddDisposable(base.ViewModel.Speed.Subscribe(delegate(string shipSpeed)
		{
			m_Speed.text = shipSpeed;
		}));
		AddDisposable(base.ViewModel.Inertia.Subscribe(delegate(string shipInertia)
		{
			m_Inertia.text = shipInertia;
		}));
		m_SpeedTooltip = new TooltipTemplateGlossary("SpeedSpace");
		m_InertiaTooltip = new TooltipTemplateGlossary("ManoeuvrabilitySpace");
		AddDisposable(m_SpeedBlock.SetGlossaryTooltip("SpeedSpace", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		AddDisposable(m_InertiaBlock.SetGlossaryTooltip("ManoeuvrabilitySpace", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, TooltipPlace.transform as RectTransform)));
		AddDisposable(base.ViewModel.Speed.Subscribe(delegate
		{
			Blink();
		}));
	}

	public void Blink()
	{
		UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
		m_AnimatedMark.gameObject.SetActive(value: true);
		m_AnimatedMark.alpha = 1f;
		m_AnimatedMark.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
