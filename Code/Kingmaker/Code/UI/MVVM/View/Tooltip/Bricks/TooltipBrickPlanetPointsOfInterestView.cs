using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPlanetPointsOfInterestView : ViewBase<TooltipBrickPlanetPointsOfInterestVM>
{
	[SerializeField]
	private Image m_PointsOfInterestImage;

	[SerializeField]
	private Sprite[] m_PoiMarks;

	[SerializeField]
	private TextMeshProUGUI m_PointsOfInterestName;

	private Dictionary<Type, Sprite> m_PoiIcons;

	public void Initialize(BlueprintPointOfInterest poi)
	{
		m_PoiIcons = new Dictionary<Type, Sprite>
		{
			{
				typeof(BlueprintPointOfInterestBookEvent),
				m_PoiMarks[0]
			},
			{
				typeof(BlueprintPointOfInterestCargo),
				m_PoiMarks[1]
			},
			{
				typeof(BlueprintPointOfInterestColonyTrait),
				m_PoiMarks[2]
			},
			{
				typeof(BlueprintPointOfInterestExpedition),
				m_PoiMarks[3]
			},
			{
				typeof(BlueprintPointOfInterestGroundOperation),
				m_PoiMarks[4]
			},
			{
				typeof(BlueprintPointOfInterestLoot),
				m_PoiMarks[5]
			},
			{
				typeof(BlueprintPointOfInterestResources),
				m_PoiMarks[6]
			},
			{
				typeof(BlueprintPointOfInterestStatCheckLoot),
				m_PoiMarks[7]
			}
		};
		base.gameObject.SetActive(value: true);
		m_PointsOfInterestImage.sprite = SetPointOfInterestIcon(poi);
		m_PointsOfInterestName.text = ((!string.IsNullOrWhiteSpace(poi.Name)) ? poi.Name : UIStrings.Instance.ExplorationTexts.GetPointOfInterestTypeName(poi));
	}

	protected override void BindViewImplementation()
	{
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private Sprite SetPointOfInterestIcon(BlueprintPointOfInterest poi)
	{
		Type type = poi.GetType();
		if (!m_PoiIcons.ContainsKey(type))
		{
			return null;
		}
		return m_PoiIcons[type];
	}
}
