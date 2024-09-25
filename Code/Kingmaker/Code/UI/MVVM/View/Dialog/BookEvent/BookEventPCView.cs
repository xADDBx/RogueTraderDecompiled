using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

[RequireComponent(typeof(DialogColorsConfig))]
public class BookEventPCView : BookEventBaseView, IHasBlueprintInfo
{
	public BlueprintScriptableObject Blueprint => Game.Instance.DialogController.Dialog;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_OpenHistoryButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ShowHistory();
		}));
		AddDisposable(m_CloseHistoryButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HideHistory();
		}));
		AddDisposable(m_SwitchHistoryButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			SwitchHistory();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (base.IsShowHistory.Value)
			{
				HideHistory();
			}
			else
			{
				EventBus.RaiseEvent(delegate(IEscMenuHandler h)
				{
					h.HandleOpen();
				});
			}
		}));
		AddDisposable(m_OpenHistoryButton.SetHint(UIStrings.Instance.BookEvent.BookEventOpenHistory, "OpenHistory"));
		AddDisposable(m_CloseHistoryButton.SetHint(UIStrings.Instance.BookEvent.BookEventCloseHistory, "CloseHistory"));
	}
}
