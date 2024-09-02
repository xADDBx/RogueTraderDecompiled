using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.UI.MVVM.View.NetLobby.Console;
using Kingmaker.UI.MVVM.View.SaveLoad.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;

public class SaveLoadConsoleView : SaveLoadBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	private SaveSlotCollectionVirtualConsoleView SlotCollectionView => m_SlotCollectionView as SaveSlotCollectionVirtualConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		GamePad.Instance.BaseLayer?.Unbind();
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.BaseLayer?.Bind();
		base.DestroyViewImplementation();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_CommonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, base.ViewModel.SaveListUpdating.Not().ToReactiveProperty()), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			SelectPrev();
		}, 14, base.ViewModel.SaveLoadMenuVM.HasFewEntities)));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			SelectNext();
		}, 15, base.ViewModel.SaveLoadMenuVM.HasFewEntities)));
		AddDisposable(inputLayer.AddAxis(Scroll, 3, repeat: true));
		SlotCollectionView.AddInput(inputLayer, m_CommonHintsWidget, base.ViewModel.SaveListUpdating, base.ViewModel.IsCurrentIronManSave);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, value * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != InputLayer && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == "SaveFullScreenshotConsoleView") && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName) && !RootUIContext.Instance.IsBugReportShow)
		{
			instance.PopLayer(InputLayer);
			instance.PushLayer(InputLayer);
		}
	}
}
