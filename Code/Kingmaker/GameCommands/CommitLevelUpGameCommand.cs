using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class CommitLevelUpGameCommand : GameCommand, IMemoryPackable<CommitLevelUpGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CommitLevelUpGameCommandFormatter : MemoryPackFormatter<CommitLevelUpGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintPath m_CareerPath;

	[JsonProperty]
	[MemoryPackInclude]
	private List<SelectionEntry> m_Selections;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CommitLevelUpGameCommand()
	{
	}

	public CommitLevelUpGameCommand([NotNull] LevelUpManager levelUpManager)
	{
		m_UnitRef = levelUpManager.TargetUnit.FromBaseUnitEntity();
		m_CareerPath = levelUpManager.Path;
		m_Selections = new List<SelectionEntry>();
		foreach (SelectionState selection in levelUpManager.Selections)
		{
			if (!(selection is SelectionStateFeature { SelectionItem: var selectionItem } selectionStateFeature))
			{
				throw new ArgumentOutOfRangeException("selectionState");
			}
			BlueprintFeature blueprintFeature = selectionItem?.Feature;
			if (blueprintFeature != null)
			{
				m_Selections.Add(new SelectionEntry(selectionStateFeature.Blueprint, selectionStateFeature.PathRank, blueprintFeature));
			}
		}
	}

	protected override void ExecuteInternal()
	{
		LevelUpManager levelUpManager = new LevelUpManager(m_UnitRef.Entity.ToBaseUnitEntity(), m_CareerPath, autoCommit: false);
		foreach (SelectionState selection2 in levelUpManager.Selections)
		{
			if (!(selection2 is SelectionStateFeature selectionStateFeature))
			{
				throw new ArgumentOutOfRangeException("selectionState");
			}
			BlueprintSelection selection = selection2.Blueprint;
			int pathRank = selection2.PathRank;
			SelectionEntry selectionEntry = m_Selections.FirstItem((SelectionEntry i) => i.Selection == selection && i.PathRank == pathRank);
			FeatureSelectionItem selectionItem = selectionStateFeature.Items.FirstItem((FeatureSelectionItem i) => i.Feature == selectionEntry?.Feature);
			if (selectionItem.Feature != null)
			{
				selectionStateFeature.Select(selectionItem);
			}
		}
		levelUpManager.Commit();
		levelUpManager.Dispose();
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUICommitChanges();
		});
	}

	static CommitLevelUpGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CommitLevelUpGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CommitLevelUpGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommitLevelUpGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CommitLevelUpGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<SelectionEntry>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<SelectionEntry>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CommitLevelUpGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_UnitRef);
		writer.WriteValue(in value.m_CareerPath);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_Selections));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CommitLevelUpGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		BlueprintPath value3;
		List<SelectionEntry> value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_UnitRef;
				value3 = value.m_CareerPath;
				value4 = value.m_Selections;
				reader.ReadPackable(ref value2);
				reader.ReadValue(ref value3);
				ListFormatter.DeserializePackable(ref reader, ref value4);
				goto IL_00cd;
			}
			value2 = reader.ReadPackable<UnitReference>();
			value3 = reader.ReadValue<BlueprintPath>();
			value4 = ListFormatter.DeserializePackable<SelectionEntry>(ref reader);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CommitLevelUpGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				value3 = null;
				value4 = null;
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_CareerPath;
				value4 = value.m_Selections;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						ListFormatter.DeserializePackable(ref reader, ref value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00cd;
			}
		}
		value = new CommitLevelUpGameCommand
		{
			m_UnitRef = value2,
			m_CareerPath = value3,
			m_Selections = value4
		};
		return;
		IL_00cd:
		value.m_UnitRef = value2;
		value.m_CareerPath = value3;
		value.m_Selections = value4;
	}
}
