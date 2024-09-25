using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class SpaceSystemInformationWindowBaseView : ViewBase<SpaceSystemInformationWindowVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SystemName;

	[Header("System Image")]
	[SerializeField]
	private Image m_SystemImage;

	[SerializeField]
	private Sprite m_UnvisitedSystemSprite;

	[SerializeField]
	private Sprite m_VisitedSystemSprite;

	[SerializeField]
	private TextMeshProUGUI m_NoDataText;

	[Header("Colonized")]
	[SerializeField]
	private TextMeshProUGUI m_ColonizeText;

	[Header("Quest")]
	[SerializeField]
	private TextMeshProUGUI m_QuestsLabel;

	[SerializeField]
	private TextMeshProUGUI m_QuestName;

	[Header("Rumour")]
	[SerializeField]
	private TextMeshProUGUI m_RumoursLabel;

	[SerializeField]
	private TextMeshProUGUI m_RumourName;

	[SerializeField]
	private TextMeshProUGUI m_GlobalMapRumourInRange;

	[Header("VisitedStatus")]
	[SerializeField]
	private TextMeshProUGUI m_VisitedStatus;

	[Header("SpaceCombat")]
	[SerializeField]
	private TextMeshProUGUI m_SpaceCombatInfoText;

	[Header("Show Hide Move")]
	[SerializeField]
	private float m_ShowPosX = 25f;

	[Header("Show Hide Move")]
	[SerializeField]
	private float m_HidePosX = -600f;

	[Header("Planets")]
	[SerializeField]
	private TextMeshProUGUI m_PlanetsLabel;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[FormerlySerializedAs("m_PlanetInfoSpaceSystemInformationWindowPCViewPrefab")]
	[SerializeField]
	private PlanetInfoSpaceSystemInformationWindowView PlanetInfoSpaceSystemInformationWindowViewPrefab;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[Header("OtherObjects")]
	[SerializeField]
	private TextMeshProUGUI m_OtherObjectsLabel;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListOtherObjects;

	[FormerlySerializedAs("m_OtherObjectsInfoSpaceSystemInformationWindowPCViewPrefab")]
	[SerializeField]
	private OtherObjectsInfoSpaceSystemInformationWindowView OtherObjectsInfoSpaceSystemInformationWindowViewPrefab;

	[Header("AdditionalAnomalies")]
	[SerializeField]
	protected WidgetListMVVM m_WidgetListAdditionalAnomalies;

	[SerializeField]
	private AdditionalAnomaliesInfoSpaceSystemInformationWindowView AdditionalAnomaliesInfoSpaceSystemInformationWindowViewPrefab;

	[Header("Scrollbar")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRectExtended;

	private RectTransform m_RectTransform;

	protected RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShowSystemWindow.Subscribe(ShowHideWindow));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.UpdateInformation, delegate
		{
			SetSystemInformation();
		}));
		m_QuestsLabel.text = string.Concat("- [ ", UIStrings.Instance.QuesJournalTexts.Quests, " ] -");
		m_RumoursLabel.text = string.Concat("- [ ", UIStrings.Instance.QuesJournalTexts.Rumours, " ] -");
		m_PlanetsLabel.text = string.Concat("- [ ", UIStrings.Instance.GlobalMap.Planets, " ] -");
		m_OtherObjectsLabel.text = string.Concat("- [ ", UIStrings.Instance.GlobalMap.AsteroidsFieldDetected, " ] -");
		m_VisitedStatus.text = UIStrings.Instance.GlobalMap.UnknownSystem;
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void ShowHideWindow(bool state)
	{
		if (!state)
		{
			RectTransform.DOAnchorPosX(m_HidePosX, 0.5f);
			base.ViewModel.ShowSystemWindow.Value = false;
			return;
		}
		RectTransform.DOAnchorPosX(m_ShowPosX + 25f, 0.5f);
		DelayedInvoker.InvokeInTime(delegate
		{
			RectTransform.DOAnchorPosX(m_ShowPosX, 0.1f);
		}, 0.5f);
		SetSystemInformation();
	}

	private void SetSystemInformation()
	{
		bool value = base.ViewModel.IsVisitedSystem.Value;
		m_SystemName.text = base.ViewModel.SectorMapObjectEntity.Value.Name ?? "";
		m_SystemImage.sprite = ((!value) ? m_UnvisitedSystemSprite : ((base.ViewModel.StarSystemSprite.Value != null) ? base.ViewModel.StarSystemSprite.Value : m_VisitedSystemSprite));
		m_NoDataText.gameObject.SetActive(!value);
		m_NoDataText.text = string.Concat("|| \\\\ >", UIStrings.Instance.QuesJournalTexts.NoData, "< ---");
		m_VisitedStatus.transform.parent.gameObject.SetActive(!value);
		DrawPlanets();
		DrawOtherObjects();
		DrawAdditionalAnomalies();
		AddColonizationStatus();
		AddSectorMapRumoursInRangeInfo();
		AddQuestsInfo();
		AddRumoursInfo();
		AddSpaceCombatInfo();
		SetScrollbarSettings();
	}

	protected void CloseInformationWindow()
	{
		base.ViewModel.CloseWindow();
	}

	private void DrawPlanets()
	{
		m_WidgetList.Clear();
		PlanetInfoSpaceSystemInformationWindowVM[] array = base.ViewModel.Planets.ToArray();
		m_PlanetsLabel.gameObject.transform.parent.gameObject.SetActive(array.Length != 0 && base.ViewModel.IsVisitedSystem.Value);
		m_WidgetList.gameObject.SetActive(base.ViewModel.IsVisitedSystem.Value);
		m_WidgetList.DrawEntries(array, PlanetInfoSpaceSystemInformationWindowViewPrefab);
		if (!(m_TooltipPlace != null))
		{
			return;
		}
		m_WidgetList.Entries.ForEach(delegate(IWidgetView e)
		{
			PlanetInfoSpaceSystemInformationWindowView planetInfoSpaceSystemInformationWindowView = e as PlanetInfoSpaceSystemInformationWindowView;
			if (planetInfoSpaceSystemInformationWindowView != null)
			{
				planetInfoSpaceSystemInformationWindowView.SetTooltipPlace(m_TooltipPlace);
			}
		});
	}

	private void DrawOtherObjects()
	{
		m_WidgetListOtherObjects.Clear();
		OtherObjectsInfoSpaceSystemInformationWindowVM[] array = base.ViewModel.OtherObjects.ToArray();
		m_OtherObjectsLabel.gameObject.transform.parent.gameObject.SetActive(array.Length != 0 && base.ViewModel.IsVisitedSystem.Value);
		m_WidgetListOtherObjects.gameObject.SetActive(base.ViewModel.IsVisitedSystem.Value);
		m_WidgetListOtherObjects.DrawEntries(array, OtherObjectsInfoSpaceSystemInformationWindowViewPrefab);
	}

	private void DrawAdditionalAnomalies()
	{
		m_WidgetListAdditionalAnomalies.Clear();
		AdditionalAnomaliesInfoSpaceSystemInformationWindowVM[] vmCollection = base.ViewModel.AdditionalAnomalies.ToArray();
		m_WidgetListAdditionalAnomalies.gameObject.SetActive(base.ViewModel.IsVisitedSystem.Value);
		m_WidgetListAdditionalAnomalies.DrawEntries(vmCollection, AdditionalAnomaliesInfoSpaceSystemInformationWindowViewPrefab);
	}

	private void SetScrollbarSettings()
	{
		m_ScrollRectExtended.ScrollToTop();
	}

	private void AddColonizationStatus()
	{
		bool value = base.ViewModel.IsVisitedSystem.Value;
		bool flag = Game.Instance.ColonizationController.GetColony(base.ViewModel.SectorMapObjectEntity.Value.View) != null;
		m_ColonizeText.gameObject.transform.parent.gameObject.SetActive(flag && value);
		if (flag)
		{
			m_ColonizeText.text = UIStrings.Instance.GlobalMap.SystemColonized;
		}
	}

	private void AddQuestsInfo()
	{
		List<QuestObjective> questsForSystem = UIUtilitySpaceQuests.GetQuestsForSystem(base.ViewModel.SectorMapObjectEntity.Value.View);
		List<QuestObjective> questsForSpaceSystem = UIUtilitySpaceQuests.GetQuestsForSpaceSystem(base.ViewModel.Area);
		bool flag = (questsForSystem != null && !questsForSystem.Empty()) || (questsForSpaceSystem != null && !questsForSpaceSystem.Empty());
		m_QuestsLabel.gameObject.transform.parent.gameObject.SetActive(flag);
		m_QuestName.gameObject.transform.parent.gameObject.SetActive(flag);
		if (flag)
		{
			List<string> questsStringList = UIUtilitySpaceQuests.GetQuestsStringList(questsForSystem, questsForSpaceSystem);
			if (questsStringList.Any())
			{
				string text = string.Join(Environment.NewLine, questsStringList);
				m_QuestName.text = text;
			}
		}
	}

	private void AddRumoursInfo()
	{
		List<QuestObjective> rumoursForSystem = UIUtilitySpaceQuests.GetRumoursForSystem(base.ViewModel.SectorMapObjectEntity.Value.View);
		m_RumoursLabel.gameObject.transform.parent.gameObject.SetActive(!rumoursForSystem.Empty() && rumoursForSystem != null);
		m_RumourName.gameObject.transform.parent.gameObject.SetActive(!rumoursForSystem.Empty() && rumoursForSystem != null);
		if (rumoursForSystem != null && rumoursForSystem.Any())
		{
			List<string> list = rumoursForSystem.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list.Any())
			{
				string text = string.Join(Environment.NewLine, list);
				m_RumourName.text = text;
			}
		}
	}

	private void AddSectorMapRumoursInRangeInfo()
	{
		List<QuestObjective> rumoursForSectorMap = UIUtilitySpaceQuests.GetRumoursForSectorMap(base.ViewModel.SectorMapObjectEntity.Value.View);
		m_GlobalMapRumourInRange.gameObject.transform.parent.gameObject.SetActive(!rumoursForSectorMap.Empty() && rumoursForSectorMap != null);
		if (rumoursForSectorMap != null && rumoursForSectorMap.Any())
		{
			List<string> list = rumoursForSectorMap.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile().Text)).Select((QuestObjective rumour, int index) => rumour.Blueprint.GetTitile().Text).ToList();
			if (list.Any())
			{
				string text = string.Join(", ", list);
				m_GlobalMapRumourInRange.text = UIStrings.Instance.GlobalMap.WithinRumourRange.Text + ": " + text;
			}
		}
	}

	private void AddSpaceCombatInfo()
	{
		bool value = base.ViewModel.IsVisitedSystem.Value;
		bool flag = base.ViewModel.GetActiveAnomalies(allAnomalies: false, BlueprintAnomaly.AnomalyObjectType.Enemy).Any();
		m_SpaceCombatInfoText.gameObject.transform.parent.gameObject.SetActive(flag && value);
		if (flag && value)
		{
			m_SpaceCombatInfoText.text = UIStrings.Instance.GlobalMap.HasEnemiesInSystem;
		}
	}
}
