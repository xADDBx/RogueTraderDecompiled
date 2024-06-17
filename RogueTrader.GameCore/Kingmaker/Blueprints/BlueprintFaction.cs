using Code.GameCore.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[HashRoot]
[TypeId("9c187edec85a6b845a4998e7cf445685")]
public class BlueprintFaction : BlueprintScriptableObject
{
	private enum EAllyFactions
	{
		None,
		AllExcludePlayer,
		AllIncludePlayer,
		FactionsList
	}

	[SerializeField]
	[FormerlySerializedAs("AttackFactions")]
	private BlueprintFactionReference[] m_AttackFactions;

	[SerializeField]
	[HideIf("IsNotListBehaviour")]
	private BlueprintFactionReference[] m_AllyFactions;

	[SerializeField]
	private EAllyFactions m_AllyFactionsBehaviour = EAllyFactions.AllExcludePlayer;

	[InfoBox(Text = "Can't be target, can't join combat")]
	public bool Peaceful;

	[InfoBox(Text = "If you add this to AttackFactions, faction will become enemy for every one. Don't use it!!!")]
	public bool AlwaysEnemy;

	[InfoBox(Text = "Unit will be always marked as enemy for others. Ignores AttackFactions")]
	public bool EnemyForEveryone;

	[InfoBox(Text = "Will not start the combat, but can be target and will join combat if was attacked")]
	public bool Neutral;

	[InfoBox(Text = "Can be manually controlled by Player")]
	public bool IsDirectlyControllable;

	[InfoBox(Text = "Can be target by enemies but will not join combat")]
	public bool NeverJoinCombat;

	private bool IsNotListBehaviour => m_AllyFactionsBehaviour != EAllyFactions.FactionsList;

	public ReferenceArrayProxy<BlueprintFaction> AttackFactions
	{
		get
		{
			BlueprintReference<BlueprintFaction>[] attackFactions = m_AttackFactions;
			return attackFactions;
		}
	}

	public ReferenceArrayProxy<BlueprintFaction> AllyFactions
	{
		get
		{
			BlueprintReference<BlueprintFaction>[] allyFactions = m_AllyFactions;
			return allyFactions;
		}
	}

	public bool IsAlly(BlueprintFaction other)
	{
		switch (m_AllyFactionsBehaviour)
		{
		case EAllyFactions.None:
			return false;
		case EAllyFactions.AllExcludePlayer:
			if (!Neutral && !other.Neutral)
			{
				return !other.IsDirectlyControllable;
			}
			return false;
		case EAllyFactions.AllIncludePlayer:
			if (!Neutral)
			{
				return !other.Neutral;
			}
			return false;
		case EAllyFactions.FactionsList:
			return m_AllyFactions.FindIndex((BlueprintFactionReference x) => x.Guid.Equals(other.AssetGuid)) != -1;
		default:
			return false;
		}
	}
}
