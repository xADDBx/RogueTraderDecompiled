using System;
using System.Collections.Generic;
using System.Net;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;

public class FeedbackPopupConfigLoader
{
	private static FeedbackPopupConfigLoader s_Instance;

	public static FeedbackPopupConfigLoader Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new FeedbackPopupConfigLoader();
			}
			return s_Instance;
		}
	}

	public FeedbackPopupItem[] Items { get; private set; }

	public Dictionary<FeedbackPopupItemType, FeedbackPopupItem> ItemsMap { get; private set; }

	public FeedbackPopupConfigLoader()
	{
		LoadConfig();
	}

	private void LoadConfig()
	{
		long num = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
		string text = $"{UIConfig.Instance.FeedbackConfig.ConfigUrl}?rand={num}";
		try
		{
			using WebClient webClient = new WebClient();
			string value = webClient.DownloadString(text);
			Items = JsonConvert.DeserializeObject<FeedbackPopupItem[]>(value);
		}
		catch
		{
			PFLog.UI.Error("Failed to get FeedbackConfig from " + text + "!");
			Items = UIConfig.Instance.FeedbackConfig.FallbackItems;
		}
		ItemsMap = new Dictionary<FeedbackPopupItemType, FeedbackPopupItem>();
		Items.ForEach(delegate(FeedbackPopupItem item)
		{
			ItemsMap[item.ItemType] = item;
		});
	}
}
