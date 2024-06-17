using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization;

namespace Kingmaker.UI.Legacy.LoadingScreen;

[Serializable]
public class CategoryPlatformHints
{
	public List<LocalizedString> GlobalHints;

	public List<LocalizedString> PCHints;

	public List<LocalizedString> ConsoleHints;

	public List<LocalizedString> FilledGlobalHints => GlobalHints.Where((LocalizedString h) => !string.IsNullOrWhiteSpace(h)).ToList();

	public List<LocalizedString> FilledPCHints => PCHints.Where((LocalizedString h) => !string.IsNullOrWhiteSpace(h)).ToList();

	public List<LocalizedString> FilledConsoleHints => ConsoleHints.Where((LocalizedString h) => !string.IsNullOrWhiteSpace(h)).ToList();
}
