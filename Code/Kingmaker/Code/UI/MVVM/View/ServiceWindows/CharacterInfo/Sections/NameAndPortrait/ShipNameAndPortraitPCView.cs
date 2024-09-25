using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class ShipNameAndPortraitPCView : CharInfoComponentView<ShipNameAndPortraitVM>
{
	[SerializeField]
	private ScrambledTMP m_StarShipName;

	[SerializeField]
	private Image m_StarShipImage;

	[SerializeField]
	private TextMeshProUGUI m_InformationLabel;

	[SerializeField]
	private TextMeshProUGUI m_StarShipDescription;

	[SerializeField]
	private FadeAnimator m_ShipPartAnimator;

	[SerializeField]
	private OwlcatButton m_NextButton;

	[SerializeField]
	private OwlcatButton m_PrevButton;

	public override void Initialize()
	{
		base.Initialize();
		m_ShipPartAnimator.Initialize();
		m_InformationLabel.text = UIStrings.Instance.CommonTexts.Information;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel.StarShipImage != null)
		{
			m_StarShipImage.sprite = base.ViewModel.StarShipImage;
		}
		if (base.ViewModel.StarShipName != null)
		{
			m_StarShipName.SetText(string.Empty, base.ViewModel.StarShipName);
		}
		if (base.ViewModel.StarShipDescription != null)
		{
			m_StarShipDescription.text = base.ViewModel.StarShipDescription;
			AddDisposable(m_StarShipDescription.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true, isEncyclopedia: false, null, 0, 0, 0, new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			})));
		}
		m_NextButton.SetInteractable(state: false);
		m_PrevButton.SetInteractable(state: false);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NextButton.SetInteractable(state: true);
		m_PrevButton.SetInteractable(state: true);
	}

	protected override void OnShow()
	{
		m_ShipPartAnimator.AppearAnimation();
	}

	protected override void OnHide()
	{
		UISounds.Instance.Sounds.Character.CharacterStatsHide.Play();
		m_ShipPartAnimator.DisappearAnimation();
	}
}
