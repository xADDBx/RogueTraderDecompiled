using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class PlanetInfoSpaceSystemInformationWindowView : ViewBase<PlanetInfoSpaceSystemInformationWindowVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private Image m_ColonizedImage;

	[Header("Resources")]
	[SerializeField]
	private PlanetResourceImageView m_ImageResourceView;

	[SerializeField]
	private GameObject m_ImageResourcePanel;

	[Header("Traits")]
	[SerializeField]
	private PlanetTraitsView m_TraitsView;

	[SerializeField]
	private GameObject m_TraitsPanel;

	[Header("Points Of Interest")]
	[SerializeField]
	private PlanetPointsOfInterestView m_PointsOfInterestView;

	[SerializeField]
	private GameObject m_PointsOfInterestPanel;

	[SerializeField]
	private GameObject m_ConsoleFocusButton;

	private readonly List<PlanetResourceImageView> m_ImageResourceViewCollection = new List<PlanetResourceImageView>();

	private readonly List<PlanetTraitsView> m_TraitsViewCollection = new List<PlanetTraitsView>();

	private readonly List<PlanetPointsOfInterestView> m_PointsOfInterestViewCollection = new List<PlanetPointsOfInterestView>();

	private RectTransform m_TooltipPlace;

	public TooltipBaseTemplate CurrentTooltipInfo;

	[HideInInspector]
	public bool IsFocused;

	private ConsoleNavigationBehaviour m_ParentNavigationBehaviour;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((PlanetInfoSpaceSystemInformationWindowVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is PlanetInfoSpaceSystemInformationWindowVM;
	}

	public void SetTooltipPlace(RectTransform tooltipPlace)
	{
		m_TooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		m_Text.text = (base.ViewModel.IsExplored ? ("- " + base.ViewModel.Name) : ("- " + UIStrings.Instance.ExplorationTexts.ExploNotExplored));
		m_Image.sprite = base.ViewModel.Icon;
		m_ColonizedImage.enabled = base.ViewModel.IsColonized;
		if (base.ViewModel.IsExplored)
		{
			SetResources();
			SetTraits();
			SetPointsOfInterest();
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_ImageResourceViewCollection.ForEach(delegate(PlanetResourceImageView v)
		{
			v.Hide();
		});
		m_TraitsViewCollection.ForEach(delegate(PlanetTraitsView v)
		{
			v.Hide();
		});
		m_PointsOfInterestViewCollection.ForEach(delegate(PlanetPointsOfInterestView v)
		{
			v.Hide();
		});
		m_ImageResourceViewCollection.Clear();
		m_TraitsViewCollection.Clear();
		m_PointsOfInterestViewCollection.Clear();
	}

	private void SetResources()
	{
		ResourceData[] resources = base.ViewModel.BlueprintPlanet.Resources;
		if (resources == null || resources.Length == 0)
		{
			m_ImageResourcePanel.SetActive(value: false);
			return;
		}
		List<ColoniesState.MinerData> miners = Game.Instance.Player.ColoniesState.Miners;
		m_ImageResourcePanel.SetActive(value: true);
		ResourceData[] array = resources;
		foreach (ResourceData resourceData in array)
		{
			if (resourceData.Count > 0)
			{
				BlueprintResource resourceBlueprint = resourceData.Resource.Get();
				bool hasMiner = miners.Any((ColoniesState.MinerData m) => m.Sso == base.ViewModel.BlueprintPlanet && m.Resource == resourceBlueprint);
				PlanetResourceImageView widget = WidgetFactory.GetWidget(m_ImageResourceView, activate: true, strictMatching: true);
				widget.Initialize(resourceBlueprint.Icon, resourceBlueprint.Name, hasMiner);
				widget.transform.SetParent(m_ImageResourcePanel.transform, worldPositionStays: false);
				widget.name = $"Resource {widget.transform.GetSiblingIndex()}";
				m_ImageResourceViewCollection.Add(widget);
			}
		}
	}

	private void SetTraits()
	{
		Colony colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Area == base.ViewModel.BlueprintStarSystemMap && data.Planet == base.ViewModel.BlueprintPlanet)?.Colony;
		if (colony == null)
		{
			m_TraitsPanel.SetActive(value: false);
			return;
		}
		m_TraitsPanel.SetActive(value: true);
		foreach (BlueprintColonyEvent startedEvent in colony.StartedEvents)
		{
			PlanetTraitsView widget = WidgetFactory.GetWidget(m_TraitsView, activate: true, strictMatching: true);
			widget.Initialize(startedEvent.Name, startedEvent.Description);
			widget.transform.SetParent(m_TraitsPanel.transform, worldPositionStays: false);
			widget.name = $"Event {widget.transform.GetSiblingIndex()}";
			m_TraitsViewCollection.Add(widget);
		}
	}

	private void SetPointsOfInterest()
	{
		BlueprintComponentsEnumerator<BasePointOfInterestComponent> components = base.ViewModel.BlueprintPlanet.GetComponents<BasePointOfInterestComponent>();
		if (components.Empty())
		{
			m_PointsOfInterestPanel.SetActive(value: false);
			return;
		}
		m_PointsOfInterestPanel.SetActive(value: true);
		foreach (BasePointOfInterestComponent item in components)
		{
			Game.Instance.Player.StarSystemsState.InteractedPoints.TryGetValue(base.ViewModel.BlueprintStarSystemMap, out var value);
			if (item.PointBlueprint.IsVisible() && (value == null || !value.Contains(item.PointBlueprint)))
			{
				PlanetPointsOfInterestView widget = WidgetFactory.GetWidget(m_PointsOfInterestView, activate: true, strictMatching: true);
				widget.Initialize(item.PointBlueprint);
				widget.transform.SetParent(m_PointsOfInterestPanel.transform, worldPositionStays: false);
				widget.name = $"PointOfInterest {widget.transform.GetSiblingIndex()}";
				m_PointsOfInterestViewCollection.Add(widget);
			}
		}
	}

	public void SetParentNavigation(ConsoleNavigationBehaviour parentNavigationBehaviour)
	{
		m_ParentNavigationBehaviour = parentNavigationBehaviour;
	}

	public void SetFocus(bool value)
	{
		m_ConsoleFocusButton.SetActive(value);
		IsFocused = value;
		if (value)
		{
			CurrentTooltipInfo = new TooltipTemplateSystemMapPlanet(null, null, base.ViewModel.BlueprintPlanet, base.ViewModel.BlueprintStarSystemMap);
			this.ShowTooltip(CurrentTooltipInfo, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace), null, m_ParentNavigationBehaviour, shouldNotHideLittleTooltip: true);
		}
	}

	public bool IsValid()
	{
		return base.gameObject.activeSelf;
	}
}
