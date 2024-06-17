using System;
using System.Collections.Generic;

namespace Kingmaker.Cheats;

[Serializable]
public class BlueprintList
{
	[Serializable]
	public class Entry
	{
		public string Name;

		public string Guid;

		public string TypeFullName;

		private Type m_Type;

		public Type Type => m_Type ?? (m_Type = Type.GetType(TypeFullName));
	}

	public List<Entry> Entries;
}
