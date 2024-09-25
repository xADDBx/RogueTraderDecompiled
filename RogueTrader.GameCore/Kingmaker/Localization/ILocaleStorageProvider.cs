using System;
using Kingmaker.Localization.Enums;

namespace Kingmaker.Localization;

public interface ILocaleStorageProvider
{
	Locale Locale { get; set; }

	event Action<Locale> Changed;
}
