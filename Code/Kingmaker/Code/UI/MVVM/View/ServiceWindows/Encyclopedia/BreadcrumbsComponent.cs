using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia;

public class BreadcrumbsComponent : MonoBehaviour
{
	[SerializeField]
	protected BreadcrumbsElement BreadcrumbsSource;

	[SerializeField]
	protected Transform m_Content;

	private readonly List<BreadcrumbsElement> m_PathView = new List<BreadcrumbsElement>();

	private INode m_CurrentNode;

	public void Prepare()
	{
		BreadcrumbsSource.Prepare();
	}

	public void DoView(INode node)
	{
		if (m_CurrentNode != node)
		{
			Dispose();
			bool isLastElement = true;
			while (node != null)
			{
				BreadcrumbsElement copyInstance = BreadcrumbsSource.GetCopyInstance();
				BlueprintEncyclopediaNode obj = (node as BlueprintEncyclopediaPage)?.ParentAsset;
				bool isFirstElement = obj == null;
				copyInstance.Initialize(m_Content, node, isFirstElement, isLastElement);
				node = obj;
				m_PathView.Add(copyInstance);
				isLastElement = false;
			}
			m_CurrentNode = node;
		}
	}

	public void Dispose()
	{
		foreach (BreadcrumbsElement item in m_PathView)
		{
			item.Dispose();
		}
		m_PathView.Clear();
	}
}
