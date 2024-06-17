using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Alignments;

public class PartUnitAlignment : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitAlignment>, IEntityPartOwner
	{
		PartUnitAlignment Alignment { get; }
	}

	private const float AlignmentAngle = 45f;

	private static readonly float TrueNeutralRadius = Mathf.Sqrt(1f / 9f);

	private static readonly float AlignmentBorderCos = Cos(22.5f);

	private static readonly Dictionary<Alignment, Vector2> AlignmentsCircle = new Dictionary<Alignment, Vector2>
	{
		{
			Alignment.TrueNeutral,
			new Vector2(0f, 0f)
		},
		{
			Alignment.ChaoticNeutral,
			new Vector2(Cos(0f), Sin(0f))
		},
		{
			Alignment.ChaoticGood,
			new Vector2(Cos(45f), Sin(45f))
		},
		{
			Alignment.NeutralGood,
			new Vector2(Cos(90f), Sin(90f))
		},
		{
			Alignment.LawfulGood,
			new Vector2(Cos(135f), Sin(135f))
		},
		{
			Alignment.LawfulNeutral,
			new Vector2(Cos(180f), Sin(180f))
		},
		{
			Alignment.LawfulEvil,
			new Vector2(Cos(225f), Sin(225f))
		},
		{
			Alignment.NeutralEvil,
			new Vector2(Cos(270f), Sin(270f))
		},
		{
			Alignment.ChaoticEvil,
			new Vector2(Cos(315f), Sin(315f))
		}
	};

	[JsonProperty]
	private Vector2 m_Vector;

	[JsonProperty]
	private AlignmentMaskType m_LockedAlignmentMask;

	[JsonProperty]
	private Alignment? m_TargetAlignment;

	[JsonProperty]
	private List<AlignmentHistoryRecord> m_History;

	private Alignment? m_Value;

	[JsonProperty]
	public readonly CountableFlag Undetectable = new CountableFlag();

	public Vector2 Vector
	{
		get
		{
			if (!Undetectable || base.Owner.IsMainCharacter)
			{
				return m_Vector;
			}
			return Vector2.zero;
		}
	}

	public Alignment Value
	{
		get
		{
			if (!Undetectable || base.Owner.IsMainCharacter)
			{
				return (m_Value ?? UpdateValue()).Value;
			}
			return Alignment.TrueNeutral;
		}
	}

	public int Radius => BlueprintRoot.Instance.Dialog.AlignmentRadius;

	public AlignmentMaskType LockMask => m_LockedAlignmentMask;

	public IEnumerable<AlignmentHistoryRecord> History
	{
		get
		{
			if ((bool)Undetectable && !base.Owner.IsMainCharacter)
			{
				return new List<AlignmentHistoryRecord>
				{
					new AlignmentHistoryRecord(Vector2.zero, AlignmentShiftDirection.TrueNeutral, null)
				};
			}
			List<AlignmentHistoryRecord> list = m_History;
			if (list == null)
			{
				List<AlignmentHistoryRecord> obj = new List<AlignmentHistoryRecord>
				{
					new AlignmentHistoryRecord(Vector2.zero, AlignmentShiftDirection.TrueNeutral, null)
				};
				List<AlignmentHistoryRecord> list2 = obj;
				m_History = obj;
				list = list2;
			}
			return list;
		}
	}

	private static float Cos(float angle)
	{
		return Mathf.Cos(angle * (MathF.PI / 180f));
	}

	private static float Sin(float angle)
	{
		return Mathf.Sin(angle * (MathF.PI / 180f));
	}

	protected override void OnAttach()
	{
		Initialize(base.Owner.Blueprint.Alignment);
	}

	public void Shift(IAlignmentShiftProvider provider)
	{
		AlignmentShift alignmentShift = provider.AlignmentShift;
		Shift(alignmentShift.Direction, alignmentShift.Value, provider);
	}

	public void Shift(AlignmentShiftDirection direction, int value, [CanBeNull] IAlignmentShiftProvider provider)
	{
		if (value != 0)
		{
			Vector2 vector = Vector;
			float num = (float)value / (float)Radius;
			Vector2 vector2 = Vector + GetDirection(direction) * num;
			if (vector2.magnitude > 1f)
			{
				vector2 = vector2.normalized;
			}
			if (direction == AlignmentShiftDirection.TrueNeutral && (AlignmentsCircle[Alignment.TrueNeutral] - Vector).magnitude < num)
			{
				vector2 = AlignmentsCircle[Alignment.TrueNeutral];
			}
			SetVector(vector2);
			UpdateValue();
			OnChanged(direction, vector, provider, raiseEvent: true);
		}
	}

	public void Initialize(Alignment alignment)
	{
		Set(alignment, raiseEvents: false);
	}

	public void Set(Alignment alignment)
	{
		Set(alignment, raiseEvents: true);
	}

	private void Set(Alignment alignment, bool raiseEvents)
	{
		Vector2 vector = Vector;
		SetVector(AlignmentsCircle[alignment]);
		UpdateValue();
		m_History?.Clear();
		OnChanged(alignment.ToDirection(), vector, null, raiseEvents);
	}

	public void LockAlignment(AlignmentMaskType lockMask, Alignment? targetAlignment)
	{
		if (lockMask == AlignmentMaskType.Any)
		{
			lockMask = AlignmentMaskType.None;
		}
		m_LockedAlignmentMask = lockMask;
		if (m_LockedAlignmentMask == AlignmentMaskType.None)
		{
			m_TargetAlignment = null;
		}
		else
		{
			m_TargetAlignment = targetAlignment;
		}
		Vector2 vector = Vector;
		SetVector(vector);
		UpdateValue();
		OnChanged(m_Value.Value.ToDirection(), vector, null, raiseEvent: true);
	}

	public void Reset(bool resetLock = false)
	{
		if (resetLock)
		{
			LockAlignment(AlignmentMaskType.None, null);
		}
		if (m_TargetAlignment.HasValue)
		{
			Set(m_TargetAlignment.Value);
			return;
		}
		Alignment alignment = Value;
		foreach (AlignmentHistoryRecord item in m_History.EmptyIfNull())
		{
			if (!item.IsEmpty())
			{
				alignment = GetAlignment(item.Position);
				break;
			}
		}
		Set(alignment);
	}

	[NotNull]
	public Alignment? UpdateValue()
	{
		return m_Value = GetAlignment(Vector, m_LockedAlignmentMask);
	}

	private void SetVector(Vector2 newVector)
	{
		m_Vector = CorrectByLockedMask(newVector);
	}

	private Vector2 CorrectByLockedMask(Vector2 newValue)
	{
		if (m_LockedAlignmentMask == AlignmentMaskType.None)
		{
			return newValue;
		}
		float num = -1f;
		float magnitude = newValue.magnitude;
		Alignment key = Alignment.TrueNeutral;
		foreach (Alignment allAlignment in m_LockedAlignmentMask.GetAllAlignments())
		{
			if (allAlignment != Alignment.TrueNeutral)
			{
				float num2 = Vector2.Dot(AlignmentsCircle[allAlignment], newValue) / magnitude;
				if (num2 > num)
				{
					num = num2;
					key = allAlignment;
				}
			}
		}
		if (num < AlignmentBorderCos)
		{
			Vector3 vector = Quaternion.RotateTowards(Quaternion.LookRotation(AlignmentsCircle[key]), Quaternion.LookRotation(newValue), 22.5f) * Vector3.forward;
			float num3 = Vector2.Dot(newValue, vector);
			newValue = ((!(num3 < 0f)) ? ((Vector2)(vector * num3)) : Vector2.zero);
		}
		return newValue;
	}

	private Vector2 GetDirection(AlignmentShiftDirection direction)
	{
		return direction switch
		{
			AlignmentShiftDirection.LawfulGood => (AlignmentsCircle[Alignment.LawfulGood] - Vector).normalized, 
			AlignmentShiftDirection.NeutralGood => (AlignmentsCircle[Alignment.NeutralGood] - Vector).normalized, 
			AlignmentShiftDirection.ChaoticGood => (AlignmentsCircle[Alignment.ChaoticGood] - Vector).normalized, 
			AlignmentShiftDirection.LawfulNeutral => (AlignmentsCircle[Alignment.LawfulNeutral] - Vector).normalized, 
			AlignmentShiftDirection.TrueNeutral => (AlignmentsCircle[Alignment.TrueNeutral] - Vector).normalized, 
			AlignmentShiftDirection.ChaoticNeutral => (AlignmentsCircle[Alignment.ChaoticNeutral] - Vector).normalized, 
			AlignmentShiftDirection.LawfulEvil => (AlignmentsCircle[Alignment.LawfulEvil] - Vector).normalized, 
			AlignmentShiftDirection.NeutralEvil => (AlignmentsCircle[Alignment.NeutralEvil] - Vector).normalized, 
			AlignmentShiftDirection.ChaoticEvil => (AlignmentsCircle[Alignment.ChaoticEvil] - Vector).normalized, 
			AlignmentShiftDirection.Good => new Vector2(0f, 1f), 
			AlignmentShiftDirection.Evil => new Vector2(0f, -1f), 
			AlignmentShiftDirection.Lawful => new Vector2(-1f, 0f), 
			AlignmentShiftDirection.Chaotic => new Vector2(1f, 0f), 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
	}

	private void OnChanged(AlignmentShiftDirection direction, Vector2 prevVector, [CanBeNull] IAlignmentShiftProvider provider, bool raiseEvent)
	{
		if (raiseEvent)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IAlignmentShiftHandler>)delegate(IAlignmentShiftHandler h)
			{
				h.HandleAligmentShift(direction, provider);
			}, isCheckRuntime: true);
			Alignment prevValue = GetAlignment(prevVector);
			if (Value != prevValue)
			{
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IAlignmentChangeHandler>)delegate(IAlignmentChangeHandler h)
				{
					h.HandleAlignmentChange(Value, prevValue);
				}, isCheckRuntime: true);
			}
		}
		m_History = m_History ?? new List<AlignmentHistoryRecord>
		{
			new AlignmentHistoryRecord(Vector2.zero, AlignmentShiftDirection.TrueNeutral, null)
		};
		m_History.Add(new AlignmentHistoryRecord(Vector, direction, provider));
	}

	private static Alignment GetAlignment(Vector2 vec, AlignmentMaskType mask = AlignmentMaskType.None)
	{
		if (!(vec.sqrMagnitude <= TrueNeutralRadius * TrueNeutralRadius))
		{
			return AlignmentsCircle.Where((KeyValuePair<Alignment, Vector2> i) => i.Key != Alignment.TrueNeutral && (mask == AlignmentMaskType.None || AlignmentExtension.MaskHasAlignment(mask, i.Key.ToMask()))).Aggregate((KeyValuePair<Alignment, Vector2> a1, KeyValuePair<Alignment, Vector2> a2) => (!((a1.Value - vec).sqrMagnitude < (a2.Value - vec).sqrMagnitude)) ? a2 : a1).Key;
		}
		return Alignment.TrueNeutral;
	}

	public static IEnumerable<Alignment> GetAlignmentsSortedByDistance(Vector2 vec)
	{
		Alignment current = GetAlignment(vec);
		return from a in AlignmentsCircle
			orderby (a.Key != current) ? (vec - a.Value).SqrMagnitude() : (-1f)
			select a.Key;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Vector);
		result.Append(ref m_LockedAlignmentMask);
		if (m_TargetAlignment.HasValue)
		{
			Alignment val2 = m_TargetAlignment.Value;
			result.Append(ref val2);
		}
		List<AlignmentHistoryRecord> history = m_History;
		if (history != null)
		{
			for (int i = 0; i < history.Count; i++)
			{
				Hash128 val3 = ClassHasher<AlignmentHistoryRecord>.GetHash128(history[i]);
				result.Append(ref val3);
			}
		}
		Hash128 val4 = ClassHasher<CountableFlag>.GetHash128(Undetectable);
		result.Append(ref val4);
		return result;
	}
}
