using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class DrawMovePredictionGameCommand : GameCommand, IMemoryPackable<DrawMovePredictionGameCommand>, IMemoryPackFormatterRegister
{
	[global::MemoryPack.Internal.Preserve]
	private sealed class DrawMovePredictionGameCommandFormatter : MemoryPackFormatter<DrawMovePredictionGameCommand>
	{
		[global::MemoryPack.Internal.Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.Serialize(ref writer, ref value);
		}

		[global::MemoryPack.Internal.Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly Path m_Path;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly float[] m_CostPerEveryCell;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly UnitCommandParams m_UnitCommandParams;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private DrawMovePredictionGameCommand()
	{
	}

	[MemoryPackConstructor]
	private DrawMovePredictionGameCommand(EntityRef<BaseUnitEntity> m_unit, [NotNull] Path m_path, [CanBeNull] float[] m_costPerEveryCell, [CanBeNull] UnitCommandParams m_unitCommandParams)
	{
		if (m_path == null)
		{
			throw new ArgumentNullException("m_path");
		}
		m_Unit = m_unit;
		m_Path = m_path;
		m_CostPerEveryCell = m_costPerEveryCell;
		m_UnitCommandParams = m_unitCommandParams;
		m_Path.Claim(this);
	}

	public DrawMovePredictionGameCommand([NotNull] BaseUnitEntity unit, [NotNull] Path path, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams)
		: this((EntityRef<BaseUnitEntity>)unit, path, costPerEveryCell, unitCommandParams)
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity baseUnitEntity = m_Unit;
		if (baseUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
			return;
		}
		InitPath(m_Path);
		UnitHelper.DrawMovePredictionLocal(baseUnitEntity, m_Path, m_CostPerEveryCell);
		m_Path.Release(this);
		if (m_UnitCommandParams != null)
		{
			UnitCommandsRunner.SetVirtualMoveCommand(baseUnitEntity, m_UnitCommandParams);
		}
	}

	private static void InitPath(Path path)
	{
		using (ProfileScope.New("DrawMovePredictionGameCommand.InitPath"))
		{
			if (path.path != null)
			{
				ListPool<GraphNode>.Release(path.path);
			}
			path.path = ListPool<GraphNode>.Claim();
			path.path.IncreaseCapacity(path.vectorPath.Count);
			foreach (Vector3 item in path.vectorPath)
			{
				path.path.Add(AstarPath.active.GetNearest(item).node);
			}
		}
	}

	public override void AfterDeserialization()
	{
		base.AfterDeserialization();
		m_UnitCommandParams?.AfterDeserialization();
	}

	static DrawMovePredictionGameCommand()
	{
		RegisterFormatter();
	}

	[global::MemoryPack.Internal.Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DrawMovePredictionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DrawMovePredictionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DrawMovePredictionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DrawMovePredictionGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<float>());
		}
	}

	[global::MemoryPack.Internal.Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DrawMovePredictionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_Unit);
		writer.WriteValue(in value.m_Path);
		writer.WriteUnmanagedArray(value.m_CostPerEveryCell);
		writer.WriteValue(in value.m_UnitCommandParams);
	}

	[global::MemoryPack.Internal.Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DrawMovePredictionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		Path value3;
		float[] value4;
		UnitCommandParams value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value3 = reader.ReadValue<Path>();
				value4 = reader.ReadUnmanagedArray<float>();
				value5 = reader.ReadValue<UnitCommandParams>();
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_Path;
				value4 = value.m_CostPerEveryCell;
				value5 = value.m_UnitCommandParams;
				reader.ReadPackable(ref value2);
				reader.ReadValue(ref value3);
				reader.ReadUnmanagedArray(ref value4);
				reader.ReadValue(ref value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DrawMovePredictionGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = null;
				value4 = null;
				value5 = null;
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_Path;
				value4 = value.m_CostPerEveryCell;
				value5 = value.m_UnitCommandParams;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanagedArray(ref value4);
						if (memberCount != 3)
						{
							reader.ReadValue(ref value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new DrawMovePredictionGameCommand(value2, value3, value4, value5);
	}
}
