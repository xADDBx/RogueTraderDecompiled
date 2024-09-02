using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbySaveSlotCollectionConsoleView : NetLobbySaveSlotCollectionBaseView, ISavesUpdatedHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private readonly BoolReactiveProperty m_ShowWaitingSaveAnim = new BoolReactiveProperty(initialValue: false);

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		m_ShowWaitingSaveAnim.Value = true;
		CreateInputImpl();
	}

	public void CreateInputImpl()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationBehaviour.AddEntityVertical(NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NetLobbySavesSlots"
		});
		AddDisposable(base.AttachedFirstValidView.Subscribe(FocusOnFirstValidSaveSlot));
		AddDisposable(m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OnBack();
		}, 9, m_ShowWaitingSaveAnim.Not().ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back));
		AddDisposable(m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 8, m_ShowWaitingSaveAnim.Not().ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader));
		AddDisposable(m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 11, m_ShowWaitingSaveAnim.Not().ToReactiveProperty()), UIStrings.Instance.SaveLoadTexts.ShowScreenshot));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void FocusOnFirstValidSaveSlot()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			foreach (IConsoleEntity entity in NavigationBehaviour.Entities)
			{
				if (entity is VirtualListElement { View: SaveSlotBaseView view } && view.IsValid())
				{
					NavigationBehaviour.FocusOnEntityManual(entity);
					m_NavigationBehaviour.FocusOnEntityManual(NavigationBehaviour);
					return;
				}
			}
			NavigationBehaviour.FocusOnFirstValidEntity();
			m_NavigationBehaviour.FocusOnEntityManual(NavigationBehaviour);
		}, 1);
	}

	public void OnSaveListUpdated()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			m_ShowWaitingSaveAnim.Value = false;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ShowWaitingSaveAnim.Value = false;
	}
}
