using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentSetListView : ViewBase<DismembermentSetListVM>
{
	public WidgetListMVVM WidgetList;

	public DismembermentSetView WidgetEntityView;

	public ToggleGroup ToggleGroup;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		DismembermentSetListVM viewModel = base.ViewModel;
		viewModel.ResetSelectedSet = (Action)Delegate.Combine(viewModel.ResetSelectedSet, new Action(OnResetSelectedSet));
		WidgetList.DrawEntries(base.ViewModel.Sets, WidgetEntityView);
	}

	private void OnResetSelectedSet()
	{
		ToggleGroup.SetAllTogglesOff();
	}

	protected override void DestroyViewImplementation()
	{
		DismembermentSetListVM viewModel = base.ViewModel;
		viewModel.ResetSelectedSet = (Action)Delegate.Remove(viewModel.ResetSelectedSet, new Action(OnResetSelectedSet));
	}
}
