using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owlcat.Runtime.Core.Logging;
using Unity.Services.Analytics;
using Unity.Services.Core;

namespace Kingmaker.QA.Analytics;

public class UnityAnalyticsTarget : OwlcatAnalytics.IAnalyticsTarget
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnityAnalyticsTarget");

	public string PrivacyUrl => AnalyticsService.Instance.PrivacyUrl;

	public UnityAnalyticsTarget()
	{
		Logger.Log("Create Unity analytics target...");
	}

	public void StartDataCollection()
	{
		StartDataCollectionAsync();
	}

	public void StopDataCollection()
	{
		AnalyticsService.Instance.StopDataCollection();
	}

	public void RequestDataDeletion()
	{
		AnalyticsService.Instance.RequestDataDeletion();
	}

	public void CustomData(string eventName, IDictionary<string, object> eventParams)
	{
	}

	private async Task StartDataCollectionAsync()
	{
		try
		{
			Logger.Log("Start unity data collection");
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				await UnityServices.InitializeAsync();
			}
			AnalyticsService.Instance.StartDataCollection();
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Start unity data collection failed.");
		}
	}
}
