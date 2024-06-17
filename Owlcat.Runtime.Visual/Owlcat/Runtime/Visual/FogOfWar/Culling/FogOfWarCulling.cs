using Unity.Collections;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

public static class FogOfWarCulling
{
	private static FogOfWarCullingSystem s_System;

	internal static void Initialize()
	{
		s_System = new FogOfWarCullingSystem();
	}

	internal static void Terminate()
	{
		s_System.Dispose();
		s_System = null;
	}

	public static void RegisterStaticBlocker(FogOfWarBlocker blocker)
	{
		s_System.RegisterStaticBlocker(blocker);
	}

	public static void UnregisterStaticBlocker(FogOfWarBlocker blocker)
	{
		s_System.UnregisterStaticBlocker(blocker);
	}

	public static void RegisterDynamicBlocker(FogOfWarBlocker blocker)
	{
		s_System.RegisterDynamicBlocker(blocker);
	}

	public static void UnregisterDynamicBlocker(FogOfWarBlocker blocker)
	{
		s_System.UnregisterDynamicBlocker(blocker);
	}

	public static void RegisterTarget(ITarget target)
	{
		s_System.RegisterTarget(target);
	}

	public static void UnregisterTarget(ITarget target)
	{
		s_System.UnregisterTarget(target);
	}

	public static void UpdateTarget(ITarget target)
	{
		s_System.UpdateTarget(target);
	}

	public static void ScheduleUpdate(in NativeList<RevealerProperties> revealers)
	{
		s_System.ScheduleUpdate(in revealers);
	}

	public static void CompleteUpdate(bool applyCullingResults)
	{
		s_System.CompleteUpdate(applyCullingResults);
	}
}
