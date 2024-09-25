using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipPostsPanelPCView : ShipPostsPanelBaseView
{
	[SerializeField]
	private ShipPostPCView[] m_ShipPostViews;

	protected override void BindViewImplementation()
	{
		if (base.ViewModel.Posts.Count != m_ShipPostViews.Length)
		{
			PFLog.UI.Error("Wrong posts count!");
			return;
		}
		for (int i = 0; i < m_ShipPostViews.Length; i++)
		{
			m_ShipPostViews[i].Bind(base.ViewModel.Posts[i]);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
