using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.LoadingScreen;

public class LoadingScreenRootVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILoadingScreen, IDialogMessageBoxUIHandler, ISubscriber, IAreaHandler
{
	public readonly ReactiveProperty<LoadingScreenVM> LoadingScreenVM = new ReactiveProperty<LoadingScreenVM>();

	private BlueprintArea m_Area;

	private readonly List<DialogMessageBoxData> m_DialogMessageBoxDatas = new List<DialogMessageBoxData>();

	public LoadingScreenRootVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeLoadingScreen();
	}

	public void ShowLoadingScreen()
	{
		LoadingScreenVM disposable = (LoadingScreenVM.Value = new LoadingScreenVM(m_Area));
		AddDisposable(disposable);
	}

	public void HideLoadingScreen()
	{
		DisposeLoadingScreen();
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return LoadingScreenVM.Value?.State ?? LoadingScreenState.Hidden;
	}

	private void DisposeLoadingScreen()
	{
		DisposeAndRemove(LoadingScreenVM);
		m_Area = null;
		foreach (DialogMessageBoxData data in m_DialogMessageBoxDatas)
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
			{
				h.HandleOpen(data.MessageText, data.BoxType, data.OnClose, data.OnLinkInvoke, data.YesLabel, data.NoLabel, data.OnTextResult, data.InputText, data.InputPlaceholder, data.WaitTime, data.MaxInputTextLength, data.LoadingProgress, data.LoadingProgressCloseTrigger, data.DontShowAgainAction);
			});
		}
		m_DialogMessageBoxDatas.Clear();
	}

	public void SetLoadingArea(BlueprintArea area)
	{
		m_Area = area;
		LoadingScreenVM.Value?.SetLoadingArea(area);
	}

	public void HandleOpen(string messageText, DialogMessageBoxBase.BoxType boxType = DialogMessageBoxBase.BoxType.Message, Action<DialogMessageBoxBase.BoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, FloatReactiveProperty loadingProgress = null, ReactiveCommand loadingProgressCloseTrigger = null, Action dontShowAgainAction = null)
	{
		if (LoadingScreenVM?.Value != null)
		{
			m_DialogMessageBoxDatas.Add(new DialogMessageBoxData(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, onTextResult, inputText, inputPlaceholder, waitTime, maxInputTextLength, loadingProgress, loadingProgressCloseTrigger, dontShowAgainAction));
		}
	}

	public void HandleClose()
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
	}
}
