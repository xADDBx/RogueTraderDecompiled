using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.BugReport.Console;

public class BugReportDuplicatesConsoleView : BugReportDuplicatesBaseView
{
	[Header("Console Hint")]
	[SerializeField]
	private ConsoleHint m_OpenHint;

	[SerializeField]
	private ConsoleHint m_MetHint;

	[SerializeField]
	private ConsoleHint m_BackHint;

	private BoolReactiveProperty m_IsShowHint = new BoolReactiveProperty();

	protected override void CreateInput()
	{
		base.CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		AddDisposable(m_BackHint.Bind(InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9)));
		m_BackHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		AddDisposable(m_OpenHint.Bind(InputLayer.AddButton(delegate
		{
			OpenUrl();
		}, 8, m_IsShowHint)));
		m_OpenHint.SetLabel(UIStrings.Instance.UIBugReport.OpenJiraTask);
		AddDisposable(m_MetHint.Bind(InputLayer.AddButton(delegate
		{
			OpenMet();
		}, 10, m_IsShowHint)));
		m_MetHint.SetLabel(UIStrings.Instance.UIBugReport.OpenMet);
	}

	protected override void UpdateNavigation()
	{
		base.UpdateNavigation();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_IsShowHint.Value = entity is BugDuplicateItemView;
	}

	private void OpenUrl()
	{
		BugDuplicateItemView bugDuplicateItemView = m_NavigationBehaviour.CurrentEntity as BugDuplicateItemView;
		if (bugDuplicateItemView != null)
		{
			bugDuplicateItemView.OpenUrl();
		}
	}

	private void OpenMet()
	{
		BugDuplicateItemView bugDuplicateItemView = m_NavigationBehaviour.CurrentEntity as BugDuplicateItemView;
		if (bugDuplicateItemView != null)
		{
			bugDuplicateItemView.OpenMet();
		}
	}
}
