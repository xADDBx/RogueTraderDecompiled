using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Serializable]
public struct CombatHudCommand
{
	[Serializable]
	public struct WriteSurfaceArgs
	{
		public Material material;

		[FormerlySerializedAs("highlight")]
		public HighlightDataSource highlightBinding;

		public SurfaceCellFilter shape;
	}

	[Serializable]
	public struct BuildSurfaceArgs
	{
		public float meshOffset;
	}

	[Serializable]
	public struct BuildOutlineArgs
	{
		public Material material;

		public Material[] additionalMaterials;

		public OutlineType lineType;

		public bool overwrite;

		public float meshOffset;

		public OutlineCellFilter shape;

		public OutlineCellFilter mask;
	}

	[Serializable]
	public struct SurfaceCellFilter
	{
		public CombatHudAreas belongToAll;

		public CombatHudAreas belongToAny;

		public CombatHudAreas notBelongToAny;

		public static implicit operator SurfaceCellFilterData(in SurfaceCellFilter value)
		{
			SurfaceCellFilterData result = default(SurfaceCellFilterData);
			result.belongToAllAreaMask = (int)value.belongToAll;
			result.belongToAnyAreasMask = (int)value.belongToAny;
			result.notBelongToAnyAreasMask = (int)value.notBelongToAny;
			return result;
		}

		public CombatHudAreas GetUsedAreas()
		{
			return belongToAll | belongToAny | notBelongToAny;
		}
	}

	[Serializable]
	public struct OutlineCellFilter
	{
		public CombatHudAreas belongToAll;

		public CombatHudAreas belongToAny;

		public CombatHudAreas notBelongToAny;

		public SurfaceBufferMask surfaceBuffer;

		public static implicit operator OutlineCellFilterData(OutlineCellFilter value)
		{
			OutlineCellFilterData result = default(OutlineCellFilterData);
			result.belongToAllAreaMask = (int)value.belongToAll;
			result.belongToAnyAreasMask = (int)value.belongToAny;
			result.notBelongToAnyAreasMask = (int)value.notBelongToAny;
			result.surfaceBuffer = value.surfaceBuffer;
			return result;
		}

		public CombatHudAreas GetUsedAreas()
		{
			return belongToAll | belongToAny | notBelongToAny;
		}
	}

	public CombatHudCommandCode code;

	public WriteSurfaceArgs writeSurfaceArgs;

	public BuildSurfaceArgs buildSurfaceArgs;

	public BuildOutlineArgs buildOutlineArgs;

	public CombatHudAreas GetUsedAreas()
	{
		switch (code)
		{
		case CombatHudCommandCode.WriteFill:
		case CombatHudCommandCode.WriteStratagemFill:
			return writeSurfaceArgs.shape.GetUsedAreas();
		case CombatHudCommandCode.BuildOutline:
			return buildOutlineArgs.shape.GetUsedAreas() | buildOutlineArgs.mask.GetUsedAreas();
		default:
			return ~(CombatHudAreas.Walkable | CombatHudAreas.Movement | CombatHudAreas.ActiveUnit | CombatHudAreas.AttackOfOpportunity | CombatHudAreas.AbilityMinRange | CombatHudAreas.AbilityMaxRange | CombatHudAreas.AbilityEffectiveRange | CombatHudAreas.AbilityPrimary | CombatHudAreas.AbilitySecondary | CombatHudAreas.StratagemAlly | CombatHudAreas.StratagemAllyIntersection | CombatHudAreas.StratagemHostile | CombatHudAreas.StratagemHostileIntersection | CombatHudAreas.SpaceMovement1 | CombatHudAreas.SpaceMovement2 | CombatHudAreas.SpaceMovement3);
		}
	}

	public void OnValidate()
	{
		if (code != CombatHudCommandCode.WriteFill && code != CombatHudCommandCode.WriteStratagemFill)
		{
			writeSurfaceArgs = default(WriteSurfaceArgs);
		}
		if (code != CombatHudCommandCode.BuildFill)
		{
			buildSurfaceArgs = default(BuildSurfaceArgs);
		}
		if (code != CombatHudCommandCode.BuildOutline)
		{
			buildOutlineArgs = default(BuildOutlineArgs);
		}
	}

	public void PushCommand(SurfaceServiceRequest request, MaterialBindingDataSource bindingDataSource, int stratagemId = -1)
	{
		switch (code)
		{
		case CombatHudCommandCode.WriteFill:
		{
			MaterialOverrides overrides2 = bindingDataSource.GetOverrides(IconOverrideSource.None, writeSurfaceArgs.highlightBinding);
			int materialId2 = request.InsertMaterial(writeSurfaceArgs.material, overrides2);
			request.CommandBuffer.WriteFill(materialId2, -1, writeSurfaceArgs.shape);
			break;
		}
		case CombatHudCommandCode.WriteStratagemFill:
		{
			MaterialOverrides overrides = bindingDataSource.GetOverrides(IconOverrideSource.Stratagem, writeSurfaceArgs.highlightBinding);
			int materialId = request.InsertMaterial(writeSurfaceArgs.material, overrides);
			request.CommandBuffer.WriteFill(materialId, stratagemId, writeSurfaceArgs.shape);
			break;
		}
		case CombatHudCommandCode.ClearFillBuffer:
			request.CommandBuffer.ClearFill();
			break;
		case CombatHudCommandCode.ClearOutlineBuffer:
			request.CommandBuffer.ClearOutline();
			break;
		case CombatHudCommandCode.BuildFill:
			request.CommandBuffer.BuildFill(new float3(0f, buildSurfaceArgs.meshOffset, 0f));
			break;
		case CombatHudCommandCode.BuildOutline:
			request.CommandBuffer.ComposeOutlineMesh(buildOutlineArgs.lineType, buildOutlineArgs.overwrite, new float3(0f, buildOutlineArgs.meshOffset, 0f), buildOutlineArgs.shape, buildOutlineArgs.mask);
			request.CommandBuffer.AppendOutlineMesh(request.InsertMaterial(buildOutlineArgs.material, default(MaterialOverrides)));
			if (buildOutlineArgs.additionalMaterials != null)
			{
				Material[] additionalMaterials = buildOutlineArgs.additionalMaterials;
				foreach (Material material in additionalMaterials)
				{
					request.CommandBuffer.AppendOutlineMesh(request.InsertMaterial(material, default(MaterialOverrides)));
				}
			}
			break;
		}
	}
}
