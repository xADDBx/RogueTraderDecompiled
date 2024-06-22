using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPlanetInfoView : TooltipBaseBrickView<TooltipBrickPlanetInfoVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private Image m_ColonizedImage;

	[Header("Resources")]
	[SerializeField]
	private TooltipBrickPlanetResourceImageView m_ImageResourceView;

	[SerializeField]
	private GameObject m_ImageResourcePanel;

	[Header("Traits")]
	[SerializeField]
	private TooltipBrickPlanetTraitsView m_TraitsView;

	[SerializeField]
	private GameObject m_TraitsPanel;

	[Header("Points Of Interest")]
	[SerializeField]
	private TooltipBrickPlanetPointsOfInterestView m_PointsOfInterestView;

	[SerializeField]
	private GameObject m_PointsOfInterestPanel;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	private readonly List<TooltipBrickPlanetResourceImageView> m_ImageResourceViewCollection = new List<TooltipBrickPlanetResourceImageView>();

	private readonly List<TooltipBrickPlanetTraitsView> m_TraitsViewCollection = new List<TooltipBrickPlanetTraitsView>();

	private readonly List<TooltipBrickPlanetPointsOfInterestView> m_PointsOfInterestViewCollection = new List<TooltipBrickPlanetPointsOfInterestView>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = (base.ViewModel.IsExplored ? base.ViewModel.Name : ((string)UIStrings.Instance.ExplorationTexts.ExploNotExplored));
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
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
		m_ImageResourceViewCollection.ForEach(delegate(TooltipBrickPlanetResourceImageView v)
		{
			v.Hide();
		});
		m_TraitsViewCollection.ForEach(delegate(TooltipBrickPlanetTraitsView v)
		{
			v.Hide();
		});
		m_PointsOfInterestViewCollection.ForEach(delegate(TooltipBrickPlanetPointsOfInterestView v)
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
				TooltipBrickPlanetResourceImageView widget = WidgetFactory.GetWidget(m_ImageResourceView, activate: true, strictMatching: true);
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
			TooltipBrickPlanetTraitsView widget = WidgetFactory.GetWidget(m_TraitsView, activate: true, strictMatching: true);
			widget.Initialize(startedEvent.Name, startedEvent.Description);
			widget.transform.SetParent(m_TraitsPanel.transform, worldPositionStays: false);
			widget.name = $"Event {widget.transform.GetSiblingIndex()}";
			m_TraitsViewCollection.Add(widget);
		}
	}

	private void SetPointsOfInterest()
	{
		IEnumerable<BasePointOfInterestComponent> components = base.ViewModel.BlueprintPlanet.GetComponents<BasePointOfInterestComponent>();
		if (components == null)
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
				TooltipBrickPlanetPointsOfInterestView widget = WidgetFactory.GetWidget(m_PointsOfInterestView, activate: true, strictMatching: true);
				widget.Initialize(item.PointBlueprint);
				widget.transform.SetParent(m_PointsOfInterestPanel.transform, worldPositionStays: false);
				widget.name = $"PointOfInterest {widget.transform.GetSiblingIndex()}";
				m_PointsOfInterestViewCollection.Add(widget);
			}
		}
	}
}
