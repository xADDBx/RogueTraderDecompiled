using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct CommandBuffer : IDisposable
{
	internal NativeList<CommandRecord> recordList;

	internal NativeList<WriteFillCommandData> writeFillCommandDataList;

	internal NativeList<BuildFillCommandData> buildFillCommandDataList;

	internal NativeList<ComposeOutlineMeshCommandData> composeOutlineMeshCommandDataList;

	internal NativeList<AppendMeshCommandData> appendMeshCommandDataList;

	public CommandBuffer(Allocator allocator)
	{
		recordList = new NativeList<CommandRecord>(allocator);
		writeFillCommandDataList = new NativeList<WriteFillCommandData>(allocator);
		buildFillCommandDataList = new NativeList<BuildFillCommandData>(allocator);
		composeOutlineMeshCommandDataList = new NativeList<ComposeOutlineMeshCommandData>(allocator);
		appendMeshCommandDataList = new NativeList<AppendMeshCommandData>(allocator);
	}

	public void CopyFrom(ref CommandBuffer src)
	{
		ref NativeList<CommandRecord> reference = ref recordList;
		NativeArray<CommandRecord> other = src.recordList.AsArray();
		reference.CopyFrom(in other);
		ref NativeList<WriteFillCommandData> reference2 = ref writeFillCommandDataList;
		NativeArray<WriteFillCommandData> other2 = src.writeFillCommandDataList.AsArray();
		reference2.CopyFrom(in other2);
		ref NativeList<BuildFillCommandData> reference3 = ref buildFillCommandDataList;
		NativeArray<BuildFillCommandData> other3 = src.buildFillCommandDataList.AsArray();
		reference3.CopyFrom(in other3);
		ref NativeList<ComposeOutlineMeshCommandData> reference4 = ref composeOutlineMeshCommandDataList;
		NativeArray<ComposeOutlineMeshCommandData> other4 = src.composeOutlineMeshCommandDataList.AsArray();
		reference4.CopyFrom(in other4);
		ref NativeList<AppendMeshCommandData> reference5 = ref appendMeshCommandDataList;
		NativeArray<AppendMeshCommandData> other5 = src.appendMeshCommandDataList.AsArray();
		reference5.CopyFrom(in other5);
	}

	public void Clear()
	{
		recordList.Clear();
		writeFillCommandDataList.Clear();
		buildFillCommandDataList.Clear();
		composeOutlineMeshCommandDataList.Clear();
		appendMeshCommandDataList.Clear();
	}

	public void Dispose()
	{
		recordList.Dispose();
		writeFillCommandDataList.Dispose();
		buildFillCommandDataList.Dispose();
		composeOutlineMeshCommandDataList.Dispose();
		appendMeshCommandDataList.Dispose();
	}

	public void WriteFill(int materialId, int shapeId, SurfaceCellFilterData selectFilter)
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.WriteFill;
		commandRecord.dataIndex = writeFillCommandDataList.Length;
		CommandRecord value = commandRecord;
		WriteFillCommandData writeFillCommandData = default(WriteFillCommandData);
		writeFillCommandData.materialId = materialId;
		writeFillCommandData.shapeId = shapeId;
		writeFillCommandData.selectFilter = selectFilter;
		WriteFillCommandData value2 = writeFillCommandData;
		recordList.Add(in value);
		writeFillCommandDataList.Add(in value2);
	}

	public void ClearFill()
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.ClearFillBuffer;
		commandRecord.dataIndex = -1;
		CommandRecord value = commandRecord;
		recordList.Add(in value);
	}

	public void ClearOutline()
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.ClearOutlineBuffer;
		commandRecord.dataIndex = -1;
		CommandRecord value = commandRecord;
		recordList.Add(in value);
	}

	public void BuildFill(float3 meshOffset)
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.BuildFill;
		commandRecord.dataIndex = buildFillCommandDataList.Length;
		CommandRecord value = commandRecord;
		BuildFillCommandData buildFillCommandData = default(BuildFillCommandData);
		buildFillCommandData.meshOffset = meshOffset;
		BuildFillCommandData value2 = buildFillCommandData;
		recordList.Add(in value);
		buildFillCommandDataList.Add(in value2);
	}

	public void ComposeOutlineMesh(OutlineType lineType, bool overwrite, float3 meshOffset, OutlineCellFilterData shape, OutlineCellFilterData mask)
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.ComposeOutlineMesh;
		commandRecord.dataIndex = composeOutlineMeshCommandDataList.Length;
		CommandRecord value = commandRecord;
		ComposeOutlineMeshCommandData composeOutlineMeshCommandData = default(ComposeOutlineMeshCommandData);
		composeOutlineMeshCommandData.lineType = lineType;
		composeOutlineMeshCommandData.overwrite = overwrite;
		composeOutlineMeshCommandData.meshOffset = meshOffset;
		composeOutlineMeshCommandData.shape = shape;
		composeOutlineMeshCommandData.mask = mask;
		ComposeOutlineMeshCommandData value2 = composeOutlineMeshCommandData;
		recordList.Add(in value);
		composeOutlineMeshCommandDataList.Add(in value2);
	}

	public void AppendOutlineMesh(int materialId)
	{
		CommandRecord commandRecord = default(CommandRecord);
		commandRecord.code = CommandCode.AppendOutlineMesh;
		commandRecord.dataIndex = appendMeshCommandDataList.Length;
		CommandRecord value = commandRecord;
		AppendMeshCommandData appendMeshCommandData = default(AppendMeshCommandData);
		appendMeshCommandData.materialId = materialId;
		AppendMeshCommandData value2 = appendMeshCommandData;
		recordList.Add(in value);
		appendMeshCommandDataList.Add(in value2);
	}
}
