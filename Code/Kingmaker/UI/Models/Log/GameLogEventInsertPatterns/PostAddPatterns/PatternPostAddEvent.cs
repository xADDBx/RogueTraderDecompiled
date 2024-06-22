using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

public abstract class PatternPostAddEvent
{
	public void Apply(List<GameLogEvent> queue, GameLogEvent @event)
	{
		ApplyImpl(queue, @event);
	}

	protected abstract void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event);
}
