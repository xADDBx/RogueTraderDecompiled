using System;

namespace Kingmaker.Visual.Animation;

public class AnimationManagerRuntimeDebugClient
{
	public Action<AnimationManager, string> UpdateAction;

	public Action<AnimationManager> DestroyAction;

	private static AnimationManagerRuntimeDebugClient s_Instance;

	public static AnimationManagerRuntimeDebugClient Instance => s_Instance ?? (s_Instance = new AnimationManagerRuntimeDebugClient());

	public static void Update(AnimationManager manager, string title)
	{
		Instance?.UpdateAction?.Invoke(manager, title);
	}

	public static void OnDestroy(AnimationManager manager)
	{
		Instance?.DestroyAction?.Invoke(manager);
	}
}
