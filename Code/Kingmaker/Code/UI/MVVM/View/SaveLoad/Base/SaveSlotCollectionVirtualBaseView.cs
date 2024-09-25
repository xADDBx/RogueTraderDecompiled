using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.UI.MVVM.View.SaveLoad.Base;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveSlotCollectionVirtualBaseView : ViewBase<SaveSlotCollectionVM>
{
	[SerializeField]
	protected SaveSlotsExpandableTitleView m_ExpandableTitleView;

	[SerializeField]
	private SaveSlotBaseView m_SaveSlotPrefab;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	public GridConsoleNavigationBehaviour NavigationBehaviour;

	public ReactiveCommand AttachedFirstValidView => m_VirtualList.AttachedFirstValidView;

	public ReactiveCollection<VirtualListElementVMBase> Saves => base.ViewModel.AllTitlesAndSlots;

	protected override void BindViewImplementation()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<SaveSlotsExpandableTitleVM>(m_ExpandableTitleView), new VirtualListElementTemplate<SaveSlotVM>(m_SaveSlotPrefab));
		AddDisposable(NavigationBehaviour = m_VirtualList.GetNavigationBehaviour());
		CreateSlotGroups();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void CreateSlotGroups()
	{
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.AllTitlesAndSlots));
		ScrollToTop();
	}

	public void ScrollToTop()
	{
		m_VirtualList.ScrollController.ForceScrollToTop();
	}
}
