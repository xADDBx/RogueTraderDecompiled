using System;
using JetBrains.Annotations;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class GlossaryEntry
{
	[NotNull]
	public string Key;

	[NotNull]
	public LocalizedString Name;

	[NotNull]
	public LocalizedString Description;
}
