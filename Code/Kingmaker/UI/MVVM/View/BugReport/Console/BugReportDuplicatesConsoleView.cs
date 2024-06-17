using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.BugReport.Console;

public class BugReportDuplicatesConsoleView : BugReportDuplicatesBaseView
{
	[Header("Console Hint")]
	[SerializeField]
	private ConsoleHint m_BackHint;

	protected override void CreateInput()
	{
		base.CreateInput();
		AddDisposable(m_BackHint.Bind(InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9)));
		m_BackHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
	}
}
