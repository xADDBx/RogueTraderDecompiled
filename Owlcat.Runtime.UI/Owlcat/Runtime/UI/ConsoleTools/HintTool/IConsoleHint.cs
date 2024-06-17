using System;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public interface IConsoleHint : IDisposable
{
	void SetLabel(string label);

	ConsoleHint GetRealHint();
}
