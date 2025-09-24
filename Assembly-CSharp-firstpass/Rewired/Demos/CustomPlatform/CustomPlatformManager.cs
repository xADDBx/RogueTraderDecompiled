using Rewired.Platforms.Custom;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

public sealed class CustomPlatformManager : MonoBehaviour, ICustomPlatformInitializer
{
	public CustomPlatformHardwareJoystickMapProvider mapProvider;

	public CustomPlatformInitOptions GetCustomPlatformInitOptions()
	{
		CustomPlatformInitOptions obj = new CustomPlatformInitOptions
		{
			platformId = 0,
			platformIdentifierString = "MyPlatform",
			hardwareJoystickMapCustomPlatformMapProvider = mapProvider
		};
		CustomPlatformConfigVars configVars = new CustomPlatformConfigVars
		{
			ignoreInputWhenAppNotInFocus = true,
			useNativeKeyboard = true,
			useNativeMouse = true
		};
		obj.inputSource = new MyPlatformInputSource(configVars);
		return obj;
	}
}
