using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentBoneListView : ViewBase<DismembermentBoneListVM>
{
	public WidgetListMVVM WidgetList;

	public DismembermentBoneView WidgetEntityView;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		WidgetList.DrawEntries(base.ViewModel.Bones.ToArray(), WidgetEntityView);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
