using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostSelectorView : ViewBase<PostSelectorVM>
{
	private List<PostEntityView> m_PostEntities;

	[SerializeField]
	private GameObject m_Content;

	protected override void BindViewImplementation()
	{
		if (m_PostEntities == null)
		{
			GetPostsEntities();
		}
		for (int i = 0; i < m_PostEntities.Count; i++)
		{
			m_PostEntities[i].Bind(base.ViewModel.Selector.EntitiesCollection[i]);
		}
	}

	private void GetPostsEntities()
	{
		m_PostEntities = m_Content.GetComponentsInChildren<PostEntityView>().ToList();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.SetEntitiesHorizontal(m_PostEntities.Select((PostEntityView e) => e).ToList());
		return gridConsoleNavigationBehaviour;
	}
}
