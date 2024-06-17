using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SpaceSystemNavigatorPopup;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup;

public class SpaceSystemNavigatorPopupPCView : ViewBase<SpaceSystemNavigatorPopupVM>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CreateWayButton;

	[SerializeField]
	private OwlcatButton m_DifficultyButton;

	[SerializeField]
	private OwlcatButton m_UpgradeWayCostButton;

	[SerializeField]
	private OwlcatButton m_TravelButton;

	[SerializeField]
	private OwlcatButton m_ClosePopupButton;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_SystemName;

	[SerializeField]
	private TextMeshProUGUI m_CreateWayText;

	[SerializeField]
	private TextMeshProUGUI m_CreateWayNoResourceText;

	[SerializeField]
	private TextMeshProUGUI m_DifficultyText;

	[SerializeField]
	private TextMeshProUGUI m_UpgradeWayNoResourceText;

	[SerializeField]
	private TextMeshProUGUI m_TravelText;

	[SerializeField]
	private TextMeshProUGUI m_SystemCreateWayCost;

	[SerializeField]
	private TextMeshProUGUI m_PlusNavigatorCostText;

	[SerializeField]
	private TextMeshProUGUI m_UpgradeWayCostText;

	[Header("Views")]
	[SerializeField]
	private GameObject m_CreateWayView;

	[SerializeField]
	private GameObject m_DifficultyView;

	[SerializeField]
	private GameObject m_TravelView;

	[SerializeField]
	private Image m_CreateWayFillingView;

	[SerializeField]
	private GameObject m_PlusNavigatorCostObject;

	[SerializeField]
	private Image m_DifficultyButtonFrameImage;

	[SerializeField]
	private Image m_DifficultyButtonHightlightedBackground;

	[SerializeField]
	private Image m_Skull1;

	[SerializeField]
	private Image m_Skull2;

	[SerializeField]
	private Image m_Skull3;

	[SerializeField]
	private Image m_UpgradeWayCostFrame;

	[SerializeField]
	private Image UpgradeWayHightlightedBackground;

	[SerializeField]
	private Image m_UpgradeWayCostFillingImage;

	[SerializeField]
	private Sprite m_RedFillingImage;

	[SerializeField]
	private Sprite m_OrangeFillingImage;

	[SerializeField]
	private Sprite m_YellowFillingImage;

	[SerializeField]
	private Sprite m_GreenFillingImage;

	[SerializeField]
	private GameObject m_QuestIcon;

	[SerializeField]
	private GameObject m_RumourIcon;

	[Header("Fade")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private CanvasGroup m_CreateWayNoResourcePanelFade;

	[SerializeField]
	private CanvasGroup m_UpgradeWayNoResourcePanelFade;

	[SerializeField]
	private FadeAnimator m_PlusNavigatorCostAimator;

	[SerializeField]
	private FadeAnimator m_UpgradeWayCostAimator;

	private Image[] m_Skulls;

	private Color[] m_SkullColors;

	private Color[] m_DifficultyColors;

	private Sprite[] m_DifficultySprites;

	private Color[] m_UpgradeWayColors;

	private Sprite[] m_UpgradeWaySprites;

	private string[] m_UpgradeWayCostTexts;

	protected override void BindViewImplementation()
	{
		m_CreateWayButton.Interactable = base.ViewModel.IsScannedFrom.Value;
		if (!base.ViewModel.IsScannedFrom.Value)
		{
			AddDisposable(m_CreateWayButton.SetHint(UIStrings.Instance.SystemMap.ScanRequired));
		}
		m_FadeAnimator.AppearAnimation();
		base.ViewModel.ClosePopupsCanvas(state: true);
		m_SystemName.text = base.ViewModel.SectorMapObject.View.Name;
		m_CreateWayText.text = UIStrings.Instance.GlobalMap.CreateWay;
		m_CreateWayNoResourceText.text = string.Concat(UIStrings.Instance.GlobalMap.NoResource, " ", UIStrings.Instance.SpaceCombatTexts.NavigatorResource);
		m_DifficultyText.text = UIStrings.Instance.GlobalMap.Route;
		m_UpgradeWayNoResourceText.text = string.Concat(UIStrings.Instance.GlobalMap.NoResource, " ", UIStrings.Instance.SpaceCombatTexts.NavigatorResource);
		m_TravelText.text = UIStrings.Instance.GlobalMap.Travel;
		ConfigurePopupDetails();
		AddDisposable(m_TravelButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			base.ViewModel.SpaceSystemTravelToSystem();
		}));
		AddDisposable(m_ClosePopupButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			base.ViewModel.SpaceSystemPopupClose();
		}));
		AddDisposable(m_CreateWayButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			CreateWay(delegate
			{
				base.ViewModel.SpaceSystemCreateWay();
			});
		}));
		AddDisposable(m_DifficultyButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			ShowHideUpgradeWayPanel(show: true);
		}));
		AddDisposable(m_UpgradeWayCostButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			UpgradeWay(delegate
			{
				HandleLowerSectorMapPassageDifficulty();
			});
		}));
		AddDisposable(base.ViewModel.WayOpenOrNot.AsObservable().Subscribe(delegate(bool o)
		{
			WayIsOpen(o);
		}));
		AddDisposable(base.ViewModel.CreateWayCost.Subscribe(delegate(int c)
		{
			m_SystemCreateWayCost.text = c.ToString();
		}));
		AddDisposable(base.ViewModel.CurrentValueOfResources.Subscribe(delegate
		{
			HandleCostChanged();
		}));
		AddDisposable(m_DifficultyButton.SetHint(string.Concat(UIStrings.Instance.GlobalMap.DifficultyPassageHint, "\n\n", UIStrings.Instance.GlobalMap.TapToUpgrade)));
		AddDisposable(m_SystemName.SetTooltip(new TooltipTemplateGlobalMapSystem(base.ViewModel.SectorMapObject.View)));
		AddDisposable(base.ViewModel.IsQuest.Subscribe(m_QuestIcon.SetActive));
		if (m_RumourIcon != null)
		{
			AddDisposable(base.ViewModel.IsRumour.Subscribe(m_RumourIcon.SetActive));
		}
	}

	private void CreateWay(Action action)
	{
		if (base.ViewModel.CurrentValueOfResources.Value < base.ViewModel.CreateWayCost.Value)
		{
			ShowNoMoneyNoHoney(m_CreateWayNoResourcePanelFade, 1f, base.ViewModel.CreateWayCost.Value);
			return;
		}
		base.ViewModel.BlockPopups(state: true);
		m_CreateWayFillingView.DOFillAmount(1f, 2f).OnComplete(delegate
		{
			action();
		});
	}

	private void ShowHideUpgradeWayPanel(bool show)
	{
		if (show)
		{
			m_UpgradeWayCostAimator.AppearAnimation();
		}
		else
		{
			m_UpgradeWayCostAimator.DisappearAnimation();
		}
	}

	private void UpgradeWay(Action onEnded)
	{
		if (base.ViewModel.CurrentValueOfResources.Value < base.ViewModel.UpgradeWayCost.Value)
		{
			ShowNoMoneyNoHoney(m_UpgradeWayNoResourcePanelFade, 1f, base.ViewModel.UpgradeWayCost.Value);
			return;
		}
		base.ViewModel.BlockPopups(state: true);
		m_UpgradeWayCostFillingImage.DOFillAmount(1f, 2f).OnComplete(delegate
		{
			base.ViewModel.SpaceSystemUpgradeWay(onEnded);
		});
	}

	private void HandleCostChanged()
	{
		int currentValueOfResourcesChangeCount = base.ViewModel.CurrentValueOfResourcesChangeCount;
		if (currentValueOfResourcesChangeCount != 0)
		{
			if (currentValueOfResourcesChangeCount > 0)
			{
				m_PlusNavigatorCostText.text = "+" + currentValueOfResourcesChangeCount;
			}
			else
			{
				m_PlusNavigatorCostText.text = currentValueOfResourcesChangeCount.ToString() ?? "";
			}
			m_PlusNavigatorCostAimator.AppearAnimation(delegate
			{
				m_PlusNavigatorCostAimator.DisappearAnimation();
			});
			Sequence s = DOTween.Sequence();
			s.Append(m_PlusNavigatorCostObject.transform.DOLocalMoveY(70f, 0.5f));
			s.AppendInterval(0.5f);
			s.Append(m_PlusNavigatorCostObject.transform.DOLocalMoveY(130f, 1f));
			s.Append(m_PlusNavigatorCostObject.transform.DOLocalMoveY(Vector3.zero.y, 0f));
			base.ViewModel.CurrentValueOfResourcesChangeCount = 0;
		}
	}

	private void WayIsOpen(bool open)
	{
		m_CreateWayView.SetActive(!open);
		m_DifficultyView.SetActive(open);
		m_TravelView.SetActive(open);
		if (open)
		{
			RefreshPassageDifficultyVisual();
		}
	}

	private void ConfigurePopupDetails()
	{
		m_Skulls = new Image[3] { m_Skull1, m_Skull2, m_Skull3 };
		m_SkullColors = new Color[3]
		{
			new Color(1f, 84f / 85f, 0f),
			new Color(1f, 0.454902f, 0f),
			new Color(1f, 0.1058824f, 0f)
		};
		m_DifficultyColors = new Color[3]
		{
			new Color(1f, 84f / 85f, 0f),
			new Color(1f, 0.454902f, 0f),
			new Color(1f, 0.1058824f, 0f)
		};
		m_DifficultySprites = new Sprite[3] { m_YellowFillingImage, m_OrangeFillingImage, m_RedFillingImage };
		m_UpgradeWayColors = new Color[3]
		{
			new Color(0.1803922f, 0.6313726f, 0.1333333f),
			new Color(1f, 84f / 85f, 0f),
			new Color(1f, 0.454902f, 0f)
		};
		m_UpgradeWaySprites = new Sprite[3] { m_GreenFillingImage, m_YellowFillingImage, m_OrangeFillingImage };
		m_UpgradeWayCostTexts = new string[3]
		{
			UIStrings.Instance.GlobalMap.UpgradeWayToSafeCost,
			string.Concat(UIStrings.Instance.GlobalMap.UpgradeWayCost, " <sprite=0 color=#FFFC00>"),
			string.Concat(UIStrings.Instance.GlobalMap.UpgradeWayCost, " <sprite=0 color=#FF7400><sprite=0 color=#FF7400>")
		};
	}

	private void RefreshPassageDifficultyVisual()
	{
		base.ViewModel.SetDifficultySkulls();
		int value = base.ViewModel.ValueOfSkulls.Value;
		if (value == 0)
		{
			m_DifficultyView.SetActive(value: false);
			return;
		}
		for (int i = 0; i < m_Skulls.Length; i++)
		{
			m_Skulls[i].gameObject.SetActive(i < value);
			m_Skulls[i].color = m_SkullColors[value - 1];
		}
		m_DifficultyButtonFrameImage.color = m_DifficultyColors[value - 1];
		m_DifficultyButtonHightlightedBackground.sprite = m_DifficultySprites[value - 1];
		m_UpgradeWayCostFrame.color = m_UpgradeWayColors[value - 1];
		m_UpgradeWayCostFillingImage.sprite = m_UpgradeWaySprites[value - 1];
		UpgradeWayHightlightedBackground.sprite = m_UpgradeWaySprites[value - 1];
		m_UpgradeWayCostText.text = m_UpgradeWayCostTexts[value - 1];
	}

	protected override void DestroyViewImplementation()
	{
		ShowHideUpgradeWayPanel(show: false);
		m_FadeAnimator.DisappearAnimation();
	}

	private void ShowNoMoneyNoHoney(CanvasGroup panel, float interval, int needMoneyCount)
	{
		base.ViewModel.NoMoneyReaction(needMoneyCount);
		Sequence s = DOTween.Sequence();
		s.Append(panel.DOFade(1f, 0.5f));
		s.AppendInterval(interval);
		s.Append(panel.DOFade(0f, 0.5f));
	}

	public void HandleLowerSectorMapPassageDifficulty()
	{
		RefreshPassageDifficultyVisual();
		ShowHideUpgradeWayPanel(show: false);
		m_UpgradeWayCostFillingImage.DOFillAmount(0f, 0f);
	}
}
