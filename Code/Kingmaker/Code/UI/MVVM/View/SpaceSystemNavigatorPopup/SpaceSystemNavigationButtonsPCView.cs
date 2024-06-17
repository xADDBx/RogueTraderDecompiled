using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup.Base;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup;

public class SpaceSystemNavigationButtonsPCView : SpaceSystemNavigationButtonsBaseView
{
	[Header("Buttons PC")]
	[SerializeField]
	private OwlcatButton m_TravelButton;

	[Header("Another PC")]
	[SerializeField]
	private TextMeshProUGUI m_TravelButtonLabel;

	[SerializeField]
	private Color[] m_TravelButtonDifficultyColorsText;

	[SerializeField]
	private FadeAnimator m_TravelLabelFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_TravelLabelText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TravelButtonLabel.text = UIStrings.Instance.GlobalMap.Travel;
		AddDisposable(m_TravelButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SpaceSystemTravelToSystem();
			m_TravelLabelFadeAnimator.DisappearAnimation();
		}));
		AddDisposable(m_TravelButton.OnHoverAsObservable().Subscribe(delegate(bool state)
		{
			if (!state)
			{
				m_TravelLabelFadeAnimator.DisappearAnimation();
			}
			else
			{
				m_TravelLabelFadeAnimator.AppearAnimation();
				m_TravelLabelText.text = TravelToSystemLabel();
			}
		}));
		AddDisposable(m_CreateWayButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CreateWay(delegate
			{
				base.ViewModel.SpaceSystemCreateWay();
			});
		}));
		m_UpgradeButtons.ForEach(delegate(OwlcatButton b)
		{
			AddDisposable(b.OnLeftClickAsObservable().Subscribe(delegate
			{
				UpgradeWay(delegate
				{
					HandleLowerSectorMapPassageDifficulty();
				}, (SectorMapPassageEntity.PassageDifficulty)m_UpgradeButtons.IndexOf(b));
			}));
		});
		AddDisposable(m_CreateWayButton.OnHoverAsObservable().Subscribe(base.ShowCreateWayButtonHoverPanel));
		m_UpgradeButtons.ForEach(delegate(OwlcatButton b)
		{
			AddDisposable(b.OnHoverAsObservable().Subscribe(delegate(bool state)
			{
				ShowUpgradeWayButtonHoverPanel(b, state);
			}));
		});
	}

	private string TravelToSystemLabel()
	{
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		if (passage == null)
		{
			return string.Empty;
		}
		BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem = base.ViewModel.BlueprintSectorMapPointStarSystem;
		if (blueprintSectorMapPointStarSystem != null && blueprintSectorMapPointStarSystem.IsFakeSystem)
		{
			return UIStrings.Instance.GlobalMap.TravelToFakeSystem;
		}
		return string.Format(UIStrings.Instance.GlobalMap.TravelToWithRoute, base.ViewModel.SystemName.Value, Environment.NewLine, "<color=#" + ColorUtility.ToHtmlStringRGBA(m_DifficultyHintPanelTextsColors[(int)passage.CurrentDifficulty]) + ">" + UIStrings.Instance.GlobalMapPassages.GetDifficultyString(passage.CurrentDifficulty) + "</color>");
	}

	protected override void WayIsOpen(bool open)
	{
		base.WayIsOpen(open);
		m_TravelButton.transform.parent.gameObject.SetActive(open);
	}

	protected override void CheckUpgradeButtonsVisible()
	{
		base.CheckUpgradeButtonsVisible();
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		m_TravelButtonLabel.color = m_TravelButtonDifficultyColorsText[(int)passage.CurrentDifficulty];
	}
}
