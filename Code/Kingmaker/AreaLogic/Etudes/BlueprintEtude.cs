using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("de1b6b4bab9b223478a7bc8835ca81d7")]
public class BlueprintEtude : BlueprintFact, IEtudeReference
{
	[SerializeField]
	private BlueprintEtudeReference m_Parent;

	public ConditionsChecker ActivationCondition;

	public ConditionsChecker CompletionCondition;

	[SerializeField]
	[Tooltip("Этот этюд не будет активироваться, если не активны этюды из этого списка. Если этот список не пуст, в этюде нельзя использовать актеров.")]
	private List<BlueprintEtudeReference> m_Synchronized = new List<BlueprintEtudeReference>();

	[SerializeField]
	[FormerlySerializedAs("m_LinkedArea")]
	private BlueprintAreaPartReference m_LinkedAreaPart;

	[SerializeField]
	private List<BlueprintCampaignReference> m_LinkedCampaigns;

	[SerializeField]
	[ShowIf("LinkedAreaPartIsBlueprintArea")]
	private bool m_IncludeAreaParts = true;

	[FormerlySerializedAs("m_LinkedMechanics")]
	[SerializeField]
	[ShowCreator]
	private List<BlueprintAreaMechanicsReference> m_AddedAreaMechanics;

	public const string NameOfAddedAreaMechanics = "m_AddedAreaMechanics";

	[FormerlySerializedAs("m_LinkedStart")]
	[SerializeField]
	[ShowCreator]
	[CreateName("{m_Name}_StartsWith")]
	private List<BlueprintEtudeReference> m_StartsWith = new List<BlueprintEtudeReference>();

	[FormerlySerializedAs("m_ChainedStart")]
	[SerializeField]
	[ShowCreator]
	[CreateName("{m_Name}_StartsOnComplete")]
	private List<BlueprintEtudeReference> m_StartsOnComplete = new List<BlueprintEtudeReference>();

	[SerializeField]
	[Tooltip("Start parent etude when current etude starts.")]
	[ShowIf("HasParent")]
	private bool m_StartsParent;

	[SerializeField]
	[ShowIf("HasParent")]
	private bool m_CompletesParent;

	[FormerlySerializedAs("m_Actors")]
	[SerializeField]
	[HideIf("IsSynchronized")]
	[ShowCreator]
	private List<BlueprintEtudeConflictingGroupReference> m_ConflictingGroups = new List<BlueprintEtudeConflictingGroupReference>();

	[HideIf("IsSynchronized")]
	public int Priority;

	public IEnumerable<BlueprintEtudeReference> StartsWith => m_StartsWith;

	public IEnumerable<BlueprintEtudeReference> StartsOnComplete => m_StartsOnComplete;

	public IEnumerable<BlueprintEtudeReference> Synchronized => m_Synchronized;

	public bool IsSynchronized => m_Synchronized.Count > 0;

	public IEnumerable<BlueprintEtudeConflictingGroupReference> ConflictingGroups => m_ConflictingGroups;

	public bool StartsParent => m_StartsParent;

	public bool CompletesParent => m_CompletesParent;

	public BlueprintEtudeReference Parent
	{
		get
		{
			return m_Parent;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_Parent = value;
			}
		}
	}

	public bool HasLinkedAreaPart => !m_LinkedAreaPart.IsEmpty();

	public IEnumerable<BlueprintAreaMechanicsReference> AddedAreaMechanics => m_AddedAreaMechanics;

	private bool HasParent => !m_Parent.IsEmpty();

	private bool LinkedAreaPartIsBlueprintArea => m_LinkedAreaPart?.Get() is BlueprintArea;

	public bool HasActors => m_ConflictingGroups.Count > 0;

	public bool IsReadOnly
	{
		get
		{
			if (m_LinkedCampaigns == null || m_LinkedCampaigns.Count == 0)
			{
				return (Parent?.Get()?.IsReadOnly).GetValueOrDefault();
			}
			return !m_LinkedCampaigns.Any((BlueprintCampaignReference _campaign) => _campaign?.Get() == Game.Instance.Player.Campaign);
		}
	}

	public BlueprintAreaPart LinkedAreaPart
	{
		get
		{
			BlueprintEtude blueprintEtude = this;
			int num = 0;
			HashSet<BlueprintEtude> hashSet = null;
			while (blueprintEtude.m_LinkedAreaPart?.Get() == null)
			{
				BlueprintEtude blueprintEtude2 = SimpleBlueprintExtendAsObject.Or(blueprintEtude, null).Parent?.Get();
				if (blueprintEtude2 == null)
				{
					break;
				}
				blueprintEtude = blueprintEtude2;
				if (num++ > 10 && hashSet == null)
				{
					hashSet = new HashSet<BlueprintEtude>();
				}
				if (hashSet != null && !hashSet.Add(blueprintEtude))
				{
					throw new Exception("Cycle in etude parents: " + string.Join(", ", hashSet));
				}
			}
			return blueprintEtude.m_LinkedAreaPart?.Get();
		}
	}

	protected override Type GetFactType()
	{
		return typeof(Etude);
	}

	public bool IsLinkedAreaPart(BlueprintAreaPart targetAreaPart)
	{
		if (targetAreaPart == null)
		{
			return false;
		}
		BlueprintAreaPart blueprintAreaPart = m_LinkedAreaPart?.Get();
		if (blueprintAreaPart == targetAreaPart)
		{
			return true;
		}
		bool result = false;
		if (blueprintAreaPart is BlueprintArea blueprintArea && m_IncludeAreaParts)
		{
			foreach (BlueprintAreaPart part in blueprintArea.GetParts())
			{
				if (part == targetAreaPart)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public EtudeReferenceType GetUsagesFor(BlueprintEtude etude)
	{
		foreach (BlueprintEtudeReference item in m_Synchronized)
		{
			if (item.Get() == etude)
			{
				return EtudeReferenceType.Synchronized;
			}
		}
		if (StartsParent && CompletesParent && Parent.Get() == etude)
		{
			return EtudeReferenceType.Start | EtudeReferenceType.Complete;
		}
		if (StartsParent && Parent.Get() == etude)
		{
			return EtudeReferenceType.Start;
		}
		if (CompletesParent && Parent.Get() == etude)
		{
			return EtudeReferenceType.Complete;
		}
		return EtudeReferenceType.None;
	}
}
