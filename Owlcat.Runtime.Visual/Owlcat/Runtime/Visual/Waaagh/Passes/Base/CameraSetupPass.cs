using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class CameraSetupPass : ScriptableRenderPass<CameraSetupPassData>
{
	private bool m_NoJitter;

	public override string Name => "CameraSetupPass";

	public CameraSetupPass(RenderPassEvent evt, bool noJitter = false)
		: base(evt)
	{
		m_NoJitter = noJitter;
	}

	protected override void Setup(RenderGraphBuilder builder, CameraSetupPassData data, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		Camera camera = cameraData.Camera;
		data.CameraRenderType = cameraData.RenderType;
		data.Camera = camera;
		if (data.CameraRenderType == CameraRenderType.Base)
		{
			PrepareCameraMatrices(data, ref cameraData, setInverseMatrices: true);
			return;
		}
		PrepareCameraMatrices(data, ref cameraData, setInverseMatrices: true);
		PrepareClippingPlanes(data, ref cameraData);
		PrepareBillboard(data, ref cameraData);
	}

	private void PrepareCameraMatrices(CameraSetupPassData data, ref CameraData cameraData, bool setInverseMatrices)
	{
		data.ViewMatrix = cameraData.GetViewMatrix();
		data.ProjectionMatrix = cameraData.GetProjectionMatrix();
		data.NonJitteredProjectionMatrix = cameraData.GetProjectionMatrixNoJitter();
		Matrix4x4 m = cameraData.GetGPUProjectionMatrix();
		data.NonJitteredGpuProjectionMatrix = cameraData.GetGPUProjectionMatrixNoJitter();
		data.NonJitteredViewProjectionMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(data.NonJitteredGpuProjectionMatrix, data.ViewMatrix, cameraData.Camera.orthographic);
		if (m_NoJitter)
		{
			m = data.NonJitteredGpuProjectionMatrix;
			data.ProjectionMatrix = data.NonJitteredProjectionMatrix;
		}
		if (setInverseMatrices)
		{
			data.InverseViewMatrix = Matrix4x4.Inverse(data.ViewMatrix);
			data.InverseProjectionMatrix = Matrix4x4.Inverse(m);
			data.InverseViewProjectionMatrix = data.InverseViewMatrix * data.InverseProjectionMatrix;
			data.WorldToCameraMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * data.ViewMatrix;
			data.CameraToWorldMatrix = data.WorldToCameraMatrix.inverse;
		}
	}

	private void PrepareClippingPlanes(CameraSetupPassData data, ref CameraData cameraData)
	{
		Matrix4x4 projMatrix = ((!m_NoJitter) ? cameraData.GetGPUProjectionMatrix() : cameraData.GetGPUProjectionMatrixNoJitter());
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		GeometryUtility.CalculateFrustumPlanes(CoreMatrixUtils.MultiplyProjectionMatrix(projMatrix, viewMatrix, cameraData.Camera.orthographic), data.CameraPlanes);
		for (int i = 0; i < data.CameraPlanes.Length; i++)
		{
			data.CameraVectorPlanes[i] = new Vector4(data.CameraPlanes[i].normal.x, data.CameraPlanes[i].normal.y, data.CameraPlanes[i].normal.z, data.CameraPlanes[i].distance);
		}
	}

	private void PrepareBillboard(CameraSetupPassData data, ref CameraData cameraData)
	{
		Matrix4x4 worldToCameraMatrix = cameraData.GetViewMatrix();
		Vector3 worldSpaceCameraPos = cameraData.WorldSpaceCameraPos;
		CalculateBillboardProperties(in worldToCameraMatrix, out var billboardTangent, out var billboardNormal, out var cameraXZAngle);
		data.BillboardNormal = new Vector4(billboardNormal.x, billboardNormal.y, billboardNormal.z, 0f);
		data.BillboardTangent = new Vector4(billboardTangent.x, billboardTangent.y, billboardTangent.z, 0f);
		data.BillboardCameraParams = new Vector4(worldSpaceCameraPos.x, worldSpaceCameraPos.y, worldSpaceCameraPos.z, cameraXZAngle);
	}

	private static void CalculateBillboardProperties(in Matrix4x4 worldToCameraMatrix, out Vector3 billboardTangent, out Vector3 billboardNormal, out float cameraXZAngle)
	{
		Matrix4x4 transpose = worldToCameraMatrix.transpose;
		Vector3 vector = new Vector3(transpose.m00, transpose.m10, transpose.m20);
		Vector3 vector2 = new Vector3(transpose.m01, transpose.m11, transpose.m21);
		Vector3 lhs = new Vector3(transpose.m02, transpose.m12, transpose.m22);
		Vector3 up = Vector3.up;
		Vector3 vector3 = Vector3.Cross(lhs, up);
		billboardTangent = ((!Mathf.Approximately(vector3.sqrMagnitude, 0f)) ? vector3.normalized : vector);
		billboardNormal = Vector3.Cross(up, billboardTangent);
		billboardNormal = ((!Mathf.Approximately(billboardNormal.sqrMagnitude, 0f)) ? billboardNormal.normalized : vector2);
		Vector3 vector4 = new Vector3(0f, 0f, 1f);
		float y = vector4.x * billboardTangent.z - vector4.z * billboardTangent.x;
		float x = vector4.x * billboardTangent.x + vector4.z * billboardTangent.z;
		cameraXZAngle = Mathf.Atan2(y, x);
		if (cameraXZAngle < 0f)
		{
			cameraXZAngle += MathF.PI * 2f;
		}
	}

	protected override void Render(CameraSetupPassData data, RenderGraphContext context)
	{
		if (data.CameraRenderType == CameraRenderType.Base)
		{
			context.renderContext.SetupCameraProperties(data.Camera);
			SetCameraMatrices(context.cmd, data, setInverseMatrices: true);
		}
		else
		{
			SetCameraMatrices(context.cmd, data, setInverseMatrices: true);
			SetClippingPlanes(context.cmd, data);
			SetBillboardProperties(context.cmd, data);
		}
	}

	private void SetCameraMatrices(CommandBuffer cmd, CameraSetupPassData data, bool setInverseMatrices)
	{
		cmd.SetViewProjectionMatrices(data.ViewMatrix, data.ProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredProjMatrix, data.NonJitteredGpuProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredViewProjMatrix, data.NonJitteredViewProjectionMatrix);
		if (setInverseMatrices)
		{
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_WorldToCamera, data.WorldToCameraMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_CameraToWorld, data.CameraToWorldMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvV, data.InverseViewMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvP, data.InverseProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvVP, data.InverseViewProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId._InvProjMatrix, data.InverseProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId._InvCameraViewProj, data.InverseViewProjectionMatrix);
			Vector3 vector = data.InverseViewMatrix.GetColumn(1);
			Vector3 vector2 = data.InverseViewMatrix.GetColumn(0);
			Vector3 vector3 = -data.InverseViewMatrix.GetColumn(2);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisUp, vector);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisSide, vector2);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisFront, vector3);
		}
	}

	private void SetClippingPlanes(CommandBuffer cmd, CameraSetupPassData data)
	{
		cmd.SetGlobalVectorArray(ShaderPropertyId.unity_CameraWorldClipPlanes, data.CameraVectorPlanes);
	}

	private void SetBillboardProperties(CommandBuffer cmd, CameraSetupPassData data)
	{
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardNormal, data.BillboardNormal);
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardTangent, data.BillboardTangent);
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardCameraParams, data.BillboardCameraParams);
	}
}
