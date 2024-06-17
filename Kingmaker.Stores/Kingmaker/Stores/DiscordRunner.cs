using System;
using Discord;
using UnityEngine;

namespace Kingmaker.Stores;

public class DiscordRunner : MonoBehaviour
{
	private const long CLIENT_ID = 543456329155477505L;

	private static DiscordRunner s_Instance;

	private global::Discord.Discord m_Discord;

	public static void Initialize()
	{
		s_Instance = new GameObject("[Discord runner]").AddComponent<DiscordRunner>();
		try
		{
			s_Instance.m_Discord = new global::Discord.Discord(543456329155477505L, 0uL);
			s_Instance.m_Discord.GetStoreManager().FetchEntitlements(delegate(Result r)
			{
				if (r != 0)
				{
					PFLog.System.Error($"Could not get DLC from discord store: {r}");
					return;
				}
				foreach (Entitlement entitlement in s_Instance.m_Discord.GetStoreManager().GetEntitlements())
				{
					PFLog.System.Log($"Discord entitlement detected: {entitlement.Type} # {entitlement.Id} (sku {entitlement.SkuId})");
				}
			});
			UnityEngine.Object.DontDestroyOnLoad(s_Instance);
		}
		catch (Exception ex)
		{
			PFLog.System.Exception(ex, "Could not initialize discord store");
			UnityEngine.Object.Destroy(s_Instance.gameObject);
		}
	}

	private void Update()
	{
		m_Discord?.RunCallbacks();
	}

	public static bool IsDLCEnabled(long id)
	{
		if (!s_Instance)
		{
			return false;
		}
		return s_Instance.m_Discord.GetStoreManager().HasSkuEntitlement(id);
	}
}
