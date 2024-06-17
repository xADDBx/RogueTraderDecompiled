using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public class OvertipSystemResourceBlockView : ViewBase<OvertipSectorResourceBlockVM>
{
	[SerializeField]
	private RectTransform m_ResourcesGroup;

	[SerializeField]
	private RectTransform m_NeedsGroup;

	[SerializeField]
	private SystemResourceItem m_ResourceItemPrefab;

	private readonly List<SystemResourceItem> m_ResourceItems = new List<SystemResourceItem>();

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		BlueprintResource key;
		int value;
		foreach (KeyValuePair<BlueprintResource, int> item in base.ViewModel.GetResourcesFromPlanet())
		{
			item.Deconstruct(out key, out value);
			BlueprintResource itemBp = key;
			int itemAmount = value;
			SystemResourceItem widget = WidgetFactory.GetWidget(m_ResourceItemPrefab);
			widget.transform.SetParent(m_ResourcesGroup, worldPositionStays: false);
			widget.InitializePlanetResource(itemBp, itemAmount);
			m_ResourceItems.Add(widget);
		}
		foreach (KeyValuePair<BlueprintResource, int> need in base.ViewModel.GetNeeds())
		{
			need.Deconstruct(out key, out value);
			BlueprintResource itemBp2 = key;
			int itemValue = value;
			SystemResourceItem widget2 = WidgetFactory.GetWidget(m_ResourceItemPrefab);
			widget2.transform.SetParent(m_NeedsGroup, worldPositionStays: false);
			widget2.Initialize(itemBp2, itemValue);
			m_ResourceItems.Add(widget2);
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_ResourceItems.ForEach(delegate(SystemResourceItem item)
		{
			WidgetFactory.DisposeWidget(item);
		});
		m_ResourceItems.Clear();
	}
}
