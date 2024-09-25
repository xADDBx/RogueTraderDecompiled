using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public static class System
{
	private sealed class Lifecycle : ScriptableObject
	{
		[UsedImplicitly]
		private void OnEnable()
		{
			SetAvailable(value: true);
		}

		[UsedImplicitly]
		private void OnDisable()
		{
			SetAvailable(value: false);
		}
	}

	private static Service s_Service;

	private static bool s_Available;

	private static bool s_Enabled;

	private static Lifecycle s_Lifecycle;

	private static Settings? s_Settings;

	private static readonly List<IOcclusionGeometryProvider> s_OcclusionGeometryProviders = new List<IOcclusionGeometryProvider>();

	private static readonly List<ITargetInfoProvider> s_TargetInfoProviders = new List<ITargetInfoProvider>(32);

	private static readonly List<ICameraInfoProvider> s_CameraInfoProvider = new List<ICameraInfoProvider>();

	internal static Service.DebugData DebugData => s_Service?.GetDebugData();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void RuntimeInitializeOnLoad()
	{
		s_Lifecycle = ScriptableObject.CreateInstance<Lifecycle>();
	}

	internal static void SetSettings(Settings settings)
	{
		s_Service?.SetSettings(settings);
	}

	internal static void RegisterOcclusionGeometry(IOcclusionGeometryProvider provider)
	{
		LogChannel.TechArt.Log("RegisterOcclusionGeometry: {0}", provider);
		s_OcclusionGeometryProviders.Add(provider);
		s_Service?.AddOcclusionGeometry(provider);
	}

	internal static void UnregisterOcclusionGeometry(IOcclusionGeometryProvider provider)
	{
		LogChannel.TechArt.Log("UnregisterOcclusionGeometry: {0}", provider);
		s_OcclusionGeometryProviders.Remove(provider);
		s_Service?.RemoveOcclusionGeometry(provider);
	}

	public static void RegisterCamera(ICameraInfoProvider provider)
	{
		s_CameraInfoProvider.Add(provider);
		s_Service?.AddCamera(provider);
	}

	public static void UnregisterCamera(ICameraInfoProvider provider)
	{
		s_CameraInfoProvider.Remove(provider);
		s_Service?.UnregisterCamera(provider);
	}

	public static void RegisterTarget(ITargetInfoProvider provider)
	{
		s_TargetInfoProviders.Add(provider);
		s_Service?.AddTarget(provider);
	}

	public static void UnregisterTarget(ITargetInfoProvider provider)
	{
		s_TargetInfoProviders.Remove(provider);
		s_Service?.RemoveTarget(provider);
	}

	internal static void SetEnabled(bool value)
	{
		if (s_Enabled != value)
		{
			s_Enabled = value;
			LogChannel.TechArt.Log($"OccluderFadeSystem: Enable status changed ({value}).");
			UpdateServiceStatus();
		}
	}

	private static void SetAvailable(bool value)
	{
		if (s_Available != value)
		{
			s_Available = value;
			LogChannel.TechArt.Log($"OccluderFadeSystem: Available status changed ({value}).");
			UpdateServiceStatus();
		}
	}

	private static void UpdateServiceStatus()
	{
		if (s_Enabled && s_Available)
		{
			if (s_Service == null)
			{
				StartService();
			}
		}
		else if (s_Service != null)
		{
			StopService();
		}
	}

	private static void StartService()
	{
		LogChannel.TechArt.Log("OccluderFadeSystem: Starting service...");
		s_Service = new Service();
		foreach (IOcclusionGeometryProvider s_OcclusionGeometryProvider in s_OcclusionGeometryProviders)
		{
			s_Service.AddOcclusionGeometry(s_OcclusionGeometryProvider);
		}
		foreach (ICameraInfoProvider item in s_CameraInfoProvider)
		{
			s_Service.AddCamera(item);
		}
		foreach (ITargetInfoProvider s_TargetInfoProvider in s_TargetInfoProviders)
		{
			s_Service.AddTarget(s_TargetInfoProvider);
		}
		PlayerLoopUtility.RegisterUpdateDelegate(typeof(PreUpdate), typeof(System), OnUpdate);
		LogChannel.TechArt.Log("OccluderFadeSystem: Service started.");
	}

	private static void StopService()
	{
		LogChannel.TechArt.Log("OccluderFadeSystem: Stopping service...");
		PlayerLoopUtility.UnregisterUpdateDelegate(typeof(PreUpdate), typeof(System));
		s_Service.Dispose();
		s_Service = null;
		LogChannel.TechArt.Log("OccluderFadeSystem: Service stopped.");
	}

	private static void OnUpdate()
	{
		s_Service.OnUpdate();
	}
}
