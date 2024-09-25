using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Etudes;

[TypeId("ed0fd01395234eedbe5de6a440b21626")]
public class EtudeBracketEnableTutorials : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	[ValidateNotEmpty]
	private BlueprintTutorial.Reference[] m_Tutorials;

	public ReferenceArrayProxy<BlueprintTutorial> Tutorials
	{
		get
		{
			BlueprintReference<BlueprintTutorial>[] tutorials = m_Tutorials;
			return tutorials;
		}
	}

	protected override void OnEnter()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.Player.Tutorial.Ensure(tutorial.GetTutorial()).EnableByEtude();
			}
		}
	}

	protected override void OnExit()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.Player.Tutorial.Ensure(tutorial.GetTutorial()).DisableByEtude();
			}
		}
	}

	protected override void OnResume()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.Player.Tutorial.Ensure(tutorial.GetTutorial()).EnableByEtude();
			}
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
