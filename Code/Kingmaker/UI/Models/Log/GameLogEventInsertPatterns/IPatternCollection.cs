using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public interface IPatternCollection
{
	IPatternCollection AddPattern(IPattern pattern);

	bool ApplyPatterns(List<GameLogEvent> eventsQueue, GameLogEvent @event);

	void Cleanup();
}
