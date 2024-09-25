using System;

namespace Kingmaker.AI.DebugUtilities;

public abstract class AILogObject
{
	protected readonly DateTime m_Time;

	public AILogObject()
	{
		m_Time = DateTime.Now;
	}

	public abstract string GetLogString();
}
