using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("92a6f568fce84956b1ea475851f47bd7")]
public class HideFactsWhileEtudePlaying : UnitFactComponentDelegate, IHiddenUnitFacts, IEtudesUpdateHandler, ISubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool Enabled;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintEtudeReference m_Etude;

	[SerializeField]
	private BlueprintRaceReference m_ReplaceRace;

	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts;

	[CanBeNull]
	private HashSet<BlueprintFact> m_CachedFacts;

	public BlueprintEtude Etude => m_Etude;

	public HashSet<BlueprintFact> Facts => m_CachedFacts ?? (m_CachedFacts = new HashSet<BlueprintFact>(m_Facts.Select((BlueprintUnitFactReference f) => f?.Get()).NotNull()));

	public BlueprintRace ReplaceRace => m_ReplaceRace;

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnDeactivate()
	{
		RequestTransientData<ComponentData>().Enabled = false;
		base.Owner.GetOptional<UnitPartHiddenFacts>()?.Remove(this);
	}

	public void OnEtudesUpdate()
	{
		Update();
	}

	private void Update()
	{
		bool flag = Game.Instance.Player.EtudesSystem.Etudes.Get(Etude)?.IsPlaying ?? false;
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (flag && !componentData.Enabled)
		{
			componentData.Enabled = true;
			base.Owner.GetOrCreate<UnitPartHiddenFacts>().Add(this);
		}
		else if (!flag && componentData.Enabled)
		{
			componentData.Enabled = false;
			base.Owner.GetOptional<UnitPartHiddenFacts>()?.Remove(this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
