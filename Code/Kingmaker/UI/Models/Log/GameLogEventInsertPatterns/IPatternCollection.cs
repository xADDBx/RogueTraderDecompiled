using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public interface IPatternCollection
{
	IPatternCollection AddPattern(PatternAddEvent pattern);

	IPatternCollection AddPattern(PatternPostAddEvent pattern);

	void ApplyPatterns(List<GameLogEvent> eventsQueue, GameLogEvent @event);

	void Cleanup();
}
