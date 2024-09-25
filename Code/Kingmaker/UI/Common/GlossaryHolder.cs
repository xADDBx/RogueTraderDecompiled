using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UI.Common;

public static class GlossaryHolder
{
	private static bool s_Initialized;

	[NotNull]
	private static readonly Dictionary<string, GlossaryEntry> s_EntriesbyKey = new Dictionary<string, GlossaryEntry>();

	[CanBeNull]
	public static GlossaryEntry GetEntry(string key)
	{
		if (!s_Initialized)
		{
			Initialize();
		}
		return s_EntriesbyKey.Get(key.ToLowerInvariant());
	}

	private static void Initialize()
	{
		s_EntriesbyKey.Clear();
		s_Initialized = true;
		List<GlossaryStrings> list = new List<GlossaryStrings>();
		list.AddRange(LocalizedTexts.Instance.Glossaries);
		list.SelectMany((GlossaryStrings g) => g.Entries).ForEach(delegate(GlossaryEntry ge)
		{
			s_EntriesbyKey[ge.Key.ToLowerInvariant()] = ge;
		});
	}
}
