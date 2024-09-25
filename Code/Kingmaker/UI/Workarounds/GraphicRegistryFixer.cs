using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Workarounds;

public class GraphicRegistryFixer : MonoBehaviour
{
	private int m_LastCount = -1;

	private IList<Graphic> m_List;

	private readonly HashSet<Graphic> m_ToRemove = new HashSet<Graphic>();

	private Canvas m_Canvas;

	private void Start()
	{
		m_List = GraphicRegistry.GetGraphicsForCanvas(m_Canvas = GetComponent<Canvas>());
	}

	public void Update()
	{
		if (m_List is List<Graphic>)
		{
			m_List = GraphicRegistry.GetGraphicsForCanvas(m_Canvas);
		}
		if (m_LastCount == m_List.Count)
		{
			return;
		}
		for (int i = 0; i < m_List.Count; i++)
		{
			if (!m_List[i].raycastTarget)
			{
				m_ToRemove.Add(m_List[i]);
			}
		}
		foreach (Graphic item in m_ToRemove)
		{
			GraphicRegistry.UnregisterGraphicForCanvas(m_Canvas, item);
		}
		m_ToRemove.Clear();
	}
}
