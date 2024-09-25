using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.Settings;
using Kingmaker.Settings.LINQ;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Unity.Services.Core;
using UnityEngine;

namespace Kingmaker.QA.Analytics;

public class OwlcatAnalytics
{
	public interface IAnalyticsTarget
	{
		string PrivacyUrl { get; }

		void StartDataCollection();

		void StopDataCollection();

		void RequestDataDeletion();

		void CustomData(string eventName, IDictionary<string, object> eventParams);
	}

	private static OwlcatAnalytics s_Instance;

	private bool m_IsInitialized;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnityAnalyticsTarget");

	private IAnalyticsTarget Target { get; set; }

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

	private async Task LoadConfiguration()
	{
		if (m_IsInitialized)
		{
			return;
		}
		Logger.Log("Analytics config started...");
		try
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				Logger.Log("Initialize UnityServices...");
				await UnityServices.InitializeAsync();
			}
			Target = new UnityAnalyticsTarget();
			m_IsInitialized = true;
			Logger.Log("Analytics config complete");
		}
		catch (Exception)
		{
			Logger.Log("Analytics config failed");
		}
	}

	public async void StartDataCollection()
	{
		await LoadConfiguration();
		if (IsOptIn && m_IsInitialized)
		{
			Target.StartDataCollection();
		}
	}

	public void StopDataCollection()
	{
		if (IsOptIn && m_IsInitialized)
		{
			Target.StopDataCollection();
		}
	}

	public void RequestDataDeletion()
	{
		if (IsOptIn && m_IsInitialized)
		{
			Target.RequestDataDeletion();
		}
	}

	private static OwlcatAnalytics Initialize()
	{
		return s_Instance = new OwlcatAnalytics();
	}

	public void OpenInBrowser()
	{
		if (IsOptIn && m_IsInitialized)
		{
			Application.OpenURL(Target.PrivacyUrl);
		}
	}

	public void CustomEvent(string Name, IDictionary<string, object> eventParams)
	{
		if (IsOptIn && m_IsInitialized)
		{
			Target.CustomData(Name, eventParams);
		}
	}
}
