using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class BoneListView : ViewBase<BoneListVM>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private BoneView m_WidgetEntityView;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Bones.ToArray(), m_WidgetEntityView);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
