using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.QuestNotification.Console;

public class QuestNotificatorConsoleView : QuestNotificatorBaseView
{
	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	private ConsoleHint m_JournalHint;

	private CompositeDisposable m_Disposable;

	private readonly BoolReactiveProperty m_IsJournalHintActive = new BoolReactiveProperty();

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	protected override void ShowNextNotification()
	{
		base.ShowNextNotification();
		if (m_Disposable == null)
		{
			m_Disposable = new CompositeDisposable();
		}
		else
		{
			m_Disposable.Clear();
		}
		InputLayer baseLayer = GamePad.Instance.BaseLayer;
		m_Disposable.Add(m_CloseHint.BindCustomAction(9, baseLayer, base.ViewModel.IsShowUp.And(m_IsJournalHintActive).ToReactiveProperty()));
		m_Disposable.Add(m_JournalHint.BindCustomAction(17, baseLayer, base.ViewModel.IsShowUp.And(m_IsJournalHintActive).ToReactiveProperty()));
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_JournalHint.SetLabel(UIStrings.Instance.QuestNotificationTexts.ToJournal);
		baseLayer.Bind();
	}

	protected override void CheckJournalButtons()
	{
		base.CheckJournalButtons();
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_IsJournalHintActive.Value = CheckActiveToJournalButtons();
		}, 1);
	}

	protected override void Hide()
	{
		base.Hide();
		m_Disposable?.Clear();
	}
}
