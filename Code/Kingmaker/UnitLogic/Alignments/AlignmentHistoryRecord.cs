using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Alignments;

public class AlignmentHistoryRecord
{
	[JsonProperty(IsReference = false)]
	public readonly Vector2 Position;

	[JsonProperty]
	public readonly AlignmentShiftDirection Direction;

	[CanBeNull]
	private IAlignmentShiftProvider m_Provider;

	[JsonProperty]
	[UsedImplicitly]
	private BlueprintScriptableObject Provider
	{
		get
		{
			return m_Provider as BlueprintScriptableObject;
		}
		set
		{
			m_Provider = value as IAlignmentShiftProvider;
		}
	}

	public string Description
	{
		get
		{
			if (m_Provider == null)
			{
				return "";
			}
			return m_Provider.AlignmentShift.Description;
		}
	}

	public bool IsEmpty()
	{
		if (Direction == AlignmentShiftDirection.TrueNeutral && Position == Vector2.zero)
		{
			return m_Provider == null;
		}
		return false;
	}

	public AlignmentHistoryRecord(Vector2 position, AlignmentShiftDirection direction, IAlignmentShiftProvider provider)
	{
		Position = position;
		Direction = direction;
		m_Provider = provider;
	}

	[JsonConstructor]
	[UsedImplicitly]
	public AlignmentHistoryRecord()
	{
	}
}
