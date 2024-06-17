using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.TMPExtention.CurvedTextMeshPro;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class SystemNameController : MonoBehaviour, ILocalizationHandler, ISubscriber
{
	[SerializeField]
	private TextMeshPro m_Text;

	private SectorMapObject m_SectorMapObject;

	private MapObjectView m_SystemMapObject;

	private BlueprintMechanicEntityFact m_Blueprint;

	public BlueprintMechanicEntityFact Blueprint => m_Blueprint;

	private bool Visible
	{
		set
		{
			m_Text.renderer.enabled = value;
		}
	}

	public void Initialize(SectorMapObject sectorMapObject)
	{
		EventBus.Subscribe(this);
		m_SectorMapObject = sectorMapObject;
		base.name = sectorMapObject.name + "_SystemName";
		if (!(m_Text == null))
		{
			m_Text.text = sectorMapObject.Blueprint?.Name;
		}
	}

	public void Unsubscribe()
	{
		EventBus.Unsubscribe(this);
	}

	public void InitializePlanet(MapObjectView systemMapObject, string systemMapObjectName, Color32 color)
	{
		m_SystemMapObject = systemMapObject;
		m_Blueprint = systemMapObject.Data.Blueprint;
		base.name = systemMapObjectName + "_PlanetName";
		if (!(m_Text == null))
		{
			m_Text.text = systemMapObjectName;
			m_Text.color = color;
		}
	}

	public void ForceUpdate()
	{
		if (!(m_Text == null))
		{
			TextProOnACurve textProOnACurve = m_Text.Or(null)?.GetComponent<TextProOnACurve>();
			if (textProOnACurve != null)
			{
				textProOnACurve.Or(null)?.ForceUpdate();
			}
		}
	}

	public void SetVisibility(bool visible)
	{
		Visible = m_SectorMapObject.Data.IsExplored && visible;
		ForceUpdate();
	}

	public void HandleLanguageChanged()
	{
		if (m_Text != null)
		{
			m_Text.text = m_SectorMapObject.Blueprint?.Name;
		}
		ForceUpdate();
	}
}
