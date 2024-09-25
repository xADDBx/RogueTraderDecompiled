using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Etudes;

[TypeId("d7f523092176498cb0b0c2e30ed8ac81")]
public class EtudeBracketEnableTutorialSingle : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintTutorial.Reference m_Tutorial;

	public BlueprintTutorial Tutorial => m_Tutorial.Get().GetTutorial();

	protected override void OnEnter()
	{
		Game.Instance.Player.Tutorial.Ensure(Tutorial).EnableByEtude();
	}

	protected override void OnExit()
	{
		Game.Instance.Player.Tutorial.Ensure(Tutorial).DisableByEtude();
	}

	protected override void OnResume()
	{
		Game.Instance.Player.Tutorial.Ensure(Tutorial).EnableByEtude();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
