using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.Controls.SelectableState;
using UnityEngine;

namespace Owlcat.Runtime.UI.Controls.Selectable;

[AddComponentMenu("UI/Owlcat/OwlcatMultiSelectable", 70)]
[SelectionBase]
[DisallowMultipleComponent]
public class OwlcatMultiSelectable : OwlcatSelectable, IOwlcatMultiLayerHolder
{
	[SerializeField]
	private List<OwlcatMultiLayer> m_MultiLayers = new List<OwlcatMultiLayer>();

	private int m_ActiveLayerIndex;

	public int ActiveLayerIndex
	{
		get
		{
			return m_ActiveLayerIndex;
		}
		set
		{
			int num = 0;
			num = ((m_MultiLayers.Count != 0) ? Mathf.Clamp(value, 0, m_MultiLayers.Count - 1) : (-1));
			if (m_ActiveLayerIndex != num)
			{
				m_ActiveLayerIndex = num;
				DoSetLayers();
				DoSetState();
			}
		}
	}

	private List<OwlcatSelectableLayerPart> CurrentLayer
	{
		get
		{
			if (ActiveLayerIndex < 0 || m_MultiLayers.Count == 0)
			{
				return null;
			}
			return m_MultiLayers[ActiveLayerIndex].Parts;
		}
	}

	public string[] MultiLayerNames
	{
		get
		{
			List<string> result = new List<string>();
			m_MultiLayers.ForEach(delegate(OwlcatMultiLayer l)
			{
				result.Add(l.LayerName);
			});
			return result.ToArray();
		}
	}

	private void DoSetLayers()
	{
		for (int i = 0; i < m_MultiLayers.Count; i++)
		{
			foreach (OwlcatSelectableLayerPart part in m_MultiLayers[i].Parts)
			{
				part.IsActive = ActiveLayerIndex == i;
			}
		}
		m_ChildSelectables.ForEach(delegate(OwlcatSelectable c)
		{
			if (c is OwlcatMultiSelectable owlcatMultiSelectable)
			{
				owlcatMultiSelectable.ActiveLayerIndex = ActiveLayerIndex;
			}
		});
	}

	protected override void Awake()
	{
		ActiveLayerIndex = m_ActiveLayerIndex;
		base.Awake();
		DoSetLayers();
		DoSetState();
	}

	protected override void DoSetState(bool instant = false)
	{
		base.DoSetState(instant);
		DoSetMultiLayerParts(base.CurrentState, instant);
	}

	private void DoSetMultiLayerParts(OwlcatSelectionState state, bool instant)
	{
		foreach (OwlcatSelectableLayerPart item in m_MultiLayers.SelectMany((OwlcatMultiLayer layer) => layer.Parts))
		{
			item.DoPartTransition(state, instant);
		}
		m_ChildSelectables.ForEach(delegate(OwlcatSelectable c)
		{
			(c as OwlcatMultiSelectable)?.DoSetMultiLayerParts(state, instant);
		});
	}

	public void SetActiveLayer(int index)
	{
		ActiveLayerIndex = index;
	}

	public void SetActiveLayer(string str)
	{
		if (m_MultiLayers.Any((OwlcatMultiLayer layer) => layer.LayerName.Equals(str)))
		{
			ActiveLayerIndex = m_MultiLayers.FindIndex((OwlcatMultiLayer layer) => layer.LayerName.Equals(str));
		}
	}

	public void AddMultiLayer()
	{
		AddMultiLayer($"Layer {m_MultiLayers.Count + 1}");
		if (ActiveLayerIndex == -1)
		{
			ActiveLayerIndex = 0;
		}
	}

	public void AddMultiLayer(string layerName)
	{
		m_MultiLayers.Add(new OwlcatMultiLayer
		{
			LayerName = layerName,
			Parts = new List<OwlcatSelectableLayerPart>()
		});
	}

	public void RemoveMultiLayer(int index)
	{
		m_MultiLayers.RemoveAt(index);
		ActiveLayerIndex = ActiveLayerIndex;
	}

	public void AddPartToMultiLayer(string layerName)
	{
		m_MultiLayers.FirstOrDefault((OwlcatMultiLayer l) => l.LayerName == layerName)?.AddPart();
	}
}
