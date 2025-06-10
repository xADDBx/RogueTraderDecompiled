using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Owlcat.Runtime.Visual;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Kingmaker.Visual;

public static class CameraStackScreenshoter
{
	public static void TakeScreenshot(RenderTexture screenshotTex)
	{
		TakeScreenshotInternal((Camera _) => screenshotTex);
	}

	public static RenderTexture TakeScreenshotNoResize()
	{
		return TakeScreenshotInternal(delegate(Camera camera)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, MakeRenderTextureGraphicsFormat(camera));
			temporary.name = $"Screenshot RT Fullscreen {Screen.width}x{Screen.height} (CameraStackScreenshoter)";
			return temporary;
		});
	}

	private static RenderTexture TakeScreenshotInternal(Func<Camera, RenderTexture> renderTextureGetter)
	{
		EventBus.RaiseEvent(delegate(ISaveManagerHandler h)
		{
			h.HandleBeforeMadeScreenshot();
		});
		using CameraStackManager.CameraStackStateChangeScope cameraStackStateChangeScope = CameraStackManager.Instance.SetTempState(CameraStackManager.CameraStackState.AllExceptUi);
		Camera firstBase = CameraStackManager.Instance.GetFirstBase();
		if (firstBase == null)
		{
			PFLog.System.ErrorWithReport("Screenshoter: failed to get camera from stack");
			cameraStackStateChangeScope.Dispose();
			return RenderTexture.GetTemporary(Screen.width, Screen.height);
		}
		bool enabled = firstBase.enabled;
		firstBase.enabled = false;
		RenderTexture targetTexture = firstBase.targetTexture;
		RenderTexture result = (firstBase.targetTexture = renderTextureGetter(firstBase));
		firstBase.Render();
		firstBase.targetTexture = targetTexture;
		firstBase.enabled = enabled;
		cameraStackStateChangeScope.Dispose();
		EventBus.RaiseEvent(delegate(ISaveManagerHandler h)
		{
			h.HandleAfterMadeScreenshot();
		});
		return result;
	}

	private static GraphicsFormat MakeRenderTextureGraphicsFormat(Camera camera)
	{
		bool flag = false;
		bool num = camera.allowHDR && WaaaghPipeline.Asset.SupportsHDR;
		HDRColorBufferPrecision hDRColorBufferPrecision = WaaaghPipeline.Asset.HDRColorBufferPrecision;
		if (num)
		{
			if (!flag && hDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
			{
				return GraphicsFormat.B10G11R11_UFloatPack32;
			}
			if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
			{
				return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
		}
		return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
	}
}
