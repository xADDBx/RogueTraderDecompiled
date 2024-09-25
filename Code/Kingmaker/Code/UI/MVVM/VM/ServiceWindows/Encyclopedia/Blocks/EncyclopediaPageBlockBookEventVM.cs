using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockBookEventVM : EncyclopediaPageBlockVM
{
	private readonly BlueprintScriptableObject m_LogEntry;

	public bool IsAnswer => m_LogEntry is BlueprintAnswer;

	public bool IsCue => m_LogEntry is BlueprintCue;

	public string Text => GetRawText();

	public EncyclopediaPageBlockBookEventVM(BlueprintEncyclopediaBookEventPage.BookEventLogBlock block)
		: base(block)
	{
		m_LogEntry = block.Entry;
	}

	private string GetRawText()
	{
		BlueprintScriptableObject logEntry = m_LogEntry;
		if (!(logEntry is BlueprintCue blueprintCue))
		{
			if (logEntry is BlueprintAnswer blueprintAnswer)
			{
				return blueprintAnswer.Text;
			}
			return string.Empty;
		}
		return blueprintCue.Text;
	}
}
