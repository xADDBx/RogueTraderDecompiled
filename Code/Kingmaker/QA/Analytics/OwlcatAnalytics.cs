using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.Settings;
using Kingmaker.Settings.LINQ;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace Kingmaker.QA.Analytics;

public class OwlcatAnalytics
{
	private static OwlcatAnalytics s_Instance;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("OwlcatAnalytics");

	public static OwlcatAnalytics Instance => s_Instance ?? Initialize();

	public bool IsOptIn
	{
		get
		{
			if ((!Application.isEditor || BuildModeUtility.ForceAnalyticsInEditor) && IsOptInConsentShown)
			{
				return SettingsRoot.Game.Main.SendGameStatistic;
			}
			return false;
		}
	}

	public bool IsOptInConsentShown => (WasTouchedSetting<bool>)SettingsRoot.Game.Main.AskedSendGameStatistic;

	private async Task StartDataCollectionAsync()
	{
		try
		{
			Logger.Log("Start data collection");
			if (IsOptIn)
			{
				if (UnityServices.State == ServicesInitializationState.Uninitialized)
				{
					await UnityServices.InitializeAsync();
				}
				AnalyticsService.Instance.StartDataCollection();
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Start data collection failed.");
		}
	}

	public void StartDataCollection()
	{
		if (IsOptIn)
		{
			StartDataCollectionAsync();
		}
	}

	public void StopDataCollection()
	{
		if (IsOptIn)
		{
			AnalyticsService.Instance.StopDataCollection();
		}
	}

	public void RequestDataDeletion()
	{
		if (IsOptIn)
		{
			AnalyticsService.Instance.RequestDataDeletion();
		}
	}

	private static OwlcatAnalytics Initialize()
	{
		return s_Instance = new OwlcatAnalytics();
	}

	public void OpenInBrowser()
	{
		if (IsOptIn)
		{
			Application.OpenURL(AnalyticsService.Instance.PrivacyUrl);
		}
	}

	public void CustomEvent(string Name, IDictionary<string, object> eventParams)
	{
		if (IsOptIn)
		{
			AnalyticsService.Instance.CustomData(Name, eventParams);
		}
	}
}
