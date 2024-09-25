using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class SetCameraShaderVariablesPassData : PassDataBase
{
	public Vector4 ProjectionParams;

	public Vector3 WorldSpaceCameraPos;

	public Vector4 ScreenParams;

	public Vector4 ScaledScreenParams;

	public Vector4 ZBufferParams;

	public Vector4 OrthoParams;

	public Vector4 ScreenSize;

	public Vector4 GlobalMipBias;

	public CameraRenderType CameraRenderType;
}
