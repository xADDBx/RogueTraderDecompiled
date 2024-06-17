using System.Text;
using Kingmaker.Inspect;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class KnowledgeLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventKnowledge>
{
	private readonly StringBuilder m_Parts = new StringBuilder();

	public void HandleEvent(GameLogEventKnowledge evt)
	{
	}

	private void CheckIsUnlocked(InspectUnitsManager.UnitInfo unitInfo, UnitInfoPart part)
	{
	}
}
