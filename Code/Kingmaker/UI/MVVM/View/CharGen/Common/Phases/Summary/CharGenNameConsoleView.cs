using Kingmaker.UI.MVVM.View.CharGen.Console.Phases;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Summary;

public class CharGenNameConsoleView : CharGenNameBaseView
{
	public override void Initialize()
	{
		base.Initialize();
		(m_MessageBoxView as CharGenChangeNameMessageBoxConsoleView)?.Initialize();
	}
}
