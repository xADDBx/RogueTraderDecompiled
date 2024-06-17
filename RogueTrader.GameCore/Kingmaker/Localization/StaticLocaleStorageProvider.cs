using System;
using Kingmaker.Localization.Enums;

namespace Kingmaker.Localization;

public class StaticLocaleStorageProvider : ILocaleStorageProvider
{
	public static readonly StaticLocaleStorageProvider Instance = new StaticLocaleStorageProvider();

	public Locale Locale
	{
		get
		{
			return Locale.enGB;
		}
		set
		{
		}
	}

	public event Action<Locale> Changed
	{
		add
		{
		}
		remove
		{
		}
	}

	private StaticLocaleStorageProvider()
	{
	}
}
