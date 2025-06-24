using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class PlayPetInterestCutsceneGameCommand : GameCommand, IMemoryPackable<PlayPetInterestCutsceneGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PlayPetInterestCutsceneGameCommandFormatter : MemoryPackFormatter<PlayPetInterestCutsceneGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PlayPetInterestCutsceneGameCommand value)
		{
			PlayPetInterestCutsceneGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PlayPetInterestCutsceneGameCommand value)
		{
			PlayPetInterestCutsceneGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public const string PET_PARAM_NAME = "Pet";

	public const string POSITION_PARAM_NAME = "Point";

	[JsonProperty]
	[MemoryPackInclude]
	private readonly CutsceneReference m_Cutscene;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly UnitReference m_Pet;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly Vector3 m_InterestPosition;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef m_InterestEntity;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PlayPetInterestCutsceneGameCommand()
	{
	}

	[MemoryPackConstructor]
	private PlayPetInterestCutsceneGameCommand(CutsceneReference m_cutscene, UnitReference m_pet, Vector3 m_interestPosition, EntityRef m_interestEntity)
	{
		m_Cutscene = m_cutscene;
		m_Pet = m_pet;
		m_InterestPosition = m_interestPosition;
		m_InterestEntity = m_interestEntity;
	}

	public PlayPetInterestCutsceneGameCommand(CutsceneReference cutscene, BaseUnitEntity unit, Vector3 interestPosition, Entity interestEntity)
		: this(cutscene, UnitReference.FromIAbstractUnitEntity(unit), interestPosition, new EntityRef(interestEntity))
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
	}

	protected override void ExecuteInternal()
	{
		CutscenePlayerView cutscenePlayer = CutscenePlayerView.Play(m_Cutscene.Get(), new ParametrizedContextSetter
		{
			AdditionalParams = 
			{
				{
					"Pet",
					(object)m_Pet
				},
				{
					"Point",
					(object)m_InterestPosition
				}
			}
		}, queued: false, m_Pet.ToBaseUnitEntity().HoldingState);
		((MapObjectEntity)m_InterestEntity.Entity).Parts.GetOptional<PetInterestPart>().RegisterCutscene(cutscenePlayer);
	}

	static PlayPetInterestCutsceneGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PlayPetInterestCutsceneGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PlayPetInterestCutsceneGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PlayPetInterestCutsceneGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PlayPetInterestCutsceneGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PlayPetInterestCutsceneGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_Cutscene);
		writer.WritePackable(in value.m_Pet);
		writer.WriteUnmanaged(in value.m_InterestPosition);
		writer.WritePackable(in value.m_InterestEntity);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PlayPetInterestCutsceneGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CutsceneReference value2;
		UnitReference value3;
		Vector3 value4;
		EntityRef value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<CutsceneReference>();
				value3 = reader.ReadPackable<UnitReference>();
				reader.ReadUnmanaged<Vector3>(out value4);
				value5 = reader.ReadPackable<EntityRef>();
			}
			else
			{
				value2 = value.m_Cutscene;
				value3 = value.m_Pet;
				value4 = value.m_InterestPosition;
				value5 = value.m_InterestEntity;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<Vector3>(out value4);
				reader.ReadPackable(ref value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayPetInterestCutsceneGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(UnitReference);
				value4 = default(Vector3);
				value5 = default(EntityRef);
			}
			else
			{
				value2 = value.m_Cutscene;
				value3 = value.m_Pet;
				value4 = value.m_InterestPosition;
				value5 = value.m_InterestEntity;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<Vector3>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new PlayPetInterestCutsceneGameCommand(value2, value3, value4, value5);
	}
}
