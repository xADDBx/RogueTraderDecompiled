using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using MemoryPack;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("c596f85ac9e041d7a0e73ec01f67c2f4")]
[MemoryPackable(GenerateType.NoGenerate)]
public sealed class BlueprintPortrait : BlueprintScriptableObject
{
	public PortraitData Data;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("BackupPortrait")]
	private BlueprintPortraitReference m_BackupPortrait;

	public bool InitiativePortrait => Data.InitiativePortrait;

	public BlueprintPortrait BackupPortrait => m_BackupPortrait?.Get();

	public Sprite SmallPortrait => Data.SmallPortrait;

	public Sprite HalfLengthPortrait => Data.HalfLengthPortrait;

	public Sprite FullLengthPortrait => Data.FullLengthPortrait;
}
