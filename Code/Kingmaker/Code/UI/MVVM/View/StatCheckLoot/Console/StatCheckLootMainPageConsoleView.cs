using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Console;

public class StatCheckLootMainPageConsoleView : StatCheckLootMainPageBaseView<StatCheckLootUnitCardConsoleView>
{
	[Header("Input")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ConsoleHintsWidget.Dispose();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnClose();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnSwitchUnit();
		}, 10), UIStrings.Instance.ExplorationTexts.StatCheckLootSwitchUnitSubHeader));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnCheckStat();
		}, 8), UIStrings.Instance.ExplorationTexts.StatCheckLootCheckStatButton, ConsoleHintsWidget.HintPosition.Left));
	}
}
