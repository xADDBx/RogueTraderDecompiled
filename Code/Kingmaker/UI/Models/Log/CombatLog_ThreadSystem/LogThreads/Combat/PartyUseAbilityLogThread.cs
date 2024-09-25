using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Base;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class PartyUseAbilityLogThread : BaseUseAbilityLogThread, IGameLogEventHandler<GameLogEventPartyUseAbility>
{
	public void HandleEvent(GameLogEventPartyUseAbility evt)
	{
		HandleUseAbility(evt.Ability, null);
	}
}
