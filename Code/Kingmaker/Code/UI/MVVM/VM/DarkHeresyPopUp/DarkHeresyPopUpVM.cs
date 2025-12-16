using System;
using Kingmaker.Stores;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.DarkHeresyPopUp;

public class DarkHeresyPopUpVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public const string Steamlink = "https://store.steampowered.com/app/3710600/Warhammer_40000_Dark_Heresy/";

	public const string GOGlink = "https://www.gog.com/en/game/warhammer_40000_dark_heresy";

	public const string EGSlink = "https://store.epicgames.com/ru/p/warhammer-40k-dark-heresy-4cf5b0";

	public const string XBOXlink = "https://www.xbox.com/en-US/games/store/warhammer-40000-dark-heresy/9p1j4tl3m04q";

	public const string PSLink = "https://store.playstation.com/en-us/concept/10014065";

	public const string PSEntitlementLink = "WH40KDARKHERESYY";

	public const string XboxEntitlementLink = "9P1J4TL3M04Q";

	public ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	private string ExceptionMessage => "Failed to open Dark Heresy store page {0}.";

	public DarkHeresyPopUpVM()
	{
		IsVisible.Value = true;
	}

	protected override void DisposeImplementation()
	{
		IsVisible.Value = false;
	}

	private string GetStoreLink()
	{
		switch (StoreManager.Store)
		{
		case StoreType.Steam:
			return "https://store.steampowered.com/app/3710600/Warhammer_40000_Dark_Heresy/";
		case StoreType.EpicGames:
			return "https://store.epicgames.com/ru/p/warhammer-40k-dark-heresy-4cf5b0";
		case StoreType.GoG:
			return "https://www.gog.com/en/game/warhammer_40000_dark_heresy";
		case StoreType.PS4:
		case StoreType.PS5:
			return "https://store.playstation.com/en-us/concept/10014065";
		case StoreType.XboxOne:
		case StoreType.XboxSeries:
			return "https://www.xbox.com/en-US/games/store/warhammer-40000-dark-heresy/9p1j4tl3m04q";
		default:
			return "https://store.steampowered.com/app/3710600/Warhammer_40000_Dark_Heresy/";
		}
	}

	public void OpenStoreToWishlist()
	{
		switch (StoreManager.Store)
		{
		case StoreType.None:
		case StoreType.Steam:
		case StoreType.GoG:
		case StoreType.Discord:
		case StoreType.EpicGames:
		case StoreType.PS5:
			Application.OpenURL(GetStoreLink());
			break;
		case StoreType.PS4:
		case StoreType.XboxOne:
		case StoreType.Nintendo:
		case StoreType.XboxSeries:
			break;
		}
	}
}
