using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;

public class SaveSlotCollectionVirtualConsoleView : SaveSlotCollectionVirtualBaseView
{
	private readonly ReactiveProperty<bool> m_HasSlot = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowDeleteButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowSaveButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowLoadButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowRenameButton = new ReactiveProperty<bool>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.SelectedSaveSlot.CombineLatest(base.ViewModel.Mode, (SaveSlotVM vm, SaveLoadMode mode) => new { vm, mode }).Subscribe(value =>
		{
			m_HasSlot.Value = value.vm != null;
			m_ShowDeleteButton.Value = value.vm?.IsActuallySaved ?? false;
			ReactiveProperty<bool> showSaveButton = m_ShowSaveButton;
			SaveSlotVM vm2 = value.vm;
			showSaveButton.Value = vm2 != null && vm2.ShowSaveLoadButton && value.mode == SaveLoadMode.Save;
			m_ShowLoadButton.Value = value.vm != null && value.mode == SaveLoadMode.Load && !value.vm.ShowDlcRequiredLabel.Value;
			m_ShowRenameButton.Value = m_HasSlot.Value && !m_ShowDeleteButton.Value;
		}));
	}

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget commonHintsWidget, BoolReactiveProperty saveListUpdating, BoolReactiveProperty isCurrentIronManSave)
	{
		UISaveLoadTexts saveLoadTexts = LocalizedTexts.Instance.UserInterfacesText.SaveLoadTexts;
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_ShowLoadButton.And(saveListUpdating.Not()).And(isCurrentIronManSave.Not()).ToReactiveProperty()), saveLoadTexts.LoadLabel));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_ShowSaveButton.And(saveListUpdating.Not()).ToReactiveProperty()), saveLoadTexts.SaveLabel));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_ShowDeleteButton.And(saveListUpdating.Not()).ToReactiveProperty()), saveLoadTexts.DeleteLabel));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_ShowRenameButton.And(saveListUpdating.Not()).ToReactiveProperty()), saveLoadTexts.RenameSave));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 11, m_HasSlot.And(saveListUpdating.Not()).ToReactiveProperty()), saveLoadTexts.ShowScreenshot));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_HasSlot.Not().And(saveListUpdating.Not()).ToReactiveProperty()), string.Concat(UIStrings.Instance.CommonTexts.Expand, "/", UIStrings.Instance.CommonTexts.Collapse)));
		AddDisposable(commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_HasSlot.Not().And(saveListUpdating.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), saveLoadTexts.DeleteCharacter));
	}
}
