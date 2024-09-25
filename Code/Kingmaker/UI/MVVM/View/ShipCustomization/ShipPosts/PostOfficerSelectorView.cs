using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostOfficerSelectorView : ViewBase<PostOfficerSelectorVM>
{
	[SerializeField]
	private PostOfficerSelectionGroup m_PostOfficerSelectionGroup;

	protected override void BindViewImplementation()
	{
		m_PostOfficerSelectionGroup.Bind(base.ViewModel.Selector);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_PostOfficerSelectionGroup.GetNavigation();
	}
}
