using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class CameraSetupPassData : PassDataBase
{
	public CameraRenderType CameraRenderType;

	public Camera Camera;

	public Matrix4x4 ViewMatrix;

	public Matrix4x4 ProjectionMatrix;

	public Matrix4x4 NonJitteredGpuProjectionMatrix;

	public Matrix4x4 NonJitteredProjectionMatrix;

	public Matrix4x4 NonJitteredViewProjectionMatrix;

	public Matrix4x4 WorldToCameraMatrix;

	public Matrix4x4 CameraToWorldMatrix;

	public Matrix4x4 InverseViewMatrix;

	public Matrix4x4 InverseProjectionMatrix;

	public Matrix4x4 InverseViewProjectionMatrix;

	public Plane[] CameraPlanes = new Plane[6];

	public Vector4[] CameraVectorPlanes = new Vector4[6];

	public Vector4 BillboardNormal;

	public Vector4 BillboardTangent;

	public Vector4 BillboardCameraParams;
}
