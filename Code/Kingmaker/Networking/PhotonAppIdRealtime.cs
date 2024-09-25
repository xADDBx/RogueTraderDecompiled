using System;
using System.Runtime.InteropServices;
using Kingmaker.Stores;
using Kingmaker.Utility.BuildModeUtils;
using Steamworks;

namespace Kingmaker.Networking;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct PhotonAppIdRealtime
{
	private const uint SteamReleaseAppId = 2186680u;

	private const string ReleaseAppIdRealtime = "8181c6c2-d32e-4863-a320-85a7e84dcc03";

	private const string DevelopmentAppIdRealtime = "631acfd2-08e2-4f45-bed7-557e5049868b";

	public bool IsRelease => "8181c6c2-d32e-4863-a320-85a7e84dcc03".Equals(Id, StringComparison.Ordinal);

	public bool IsDevelopment => "631acfd2-08e2-4f45-bed7-557e5049868b".Equals(Id, StringComparison.Ordinal);

	public string Id
	{
		get
		{
			if (!string.IsNullOrEmpty(BuildModeUtility.Data.AppIdRealtime))
			{
				return BuildModeUtility.Data.AppIdRealtime;
			}
			if (StoreManager.Store == StoreType.Steam)
			{
				if (!SteamManager.Initialized)
				{
					throw new StoreNotInitializedException();
				}
				if (SteamUtils.GetAppID().m_AppId != 2186680)
				{
					return "631acfd2-08e2-4f45-bed7-557e5049868b";
				}
			}
			return "8181c6c2-d32e-4863-a320-85a7e84dcc03";
		}
	}
}
