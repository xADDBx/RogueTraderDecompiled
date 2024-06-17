using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public interface IPattern
{
	bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event);
}
