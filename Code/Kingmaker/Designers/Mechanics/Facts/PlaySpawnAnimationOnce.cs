using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Play spawn animation once")]
[TypeId("33d24a2e57bb73049b088fc27a3be9cd")]
public class PlaySpawnAnimationOnce : UnitFactComponentDelegate, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool Played;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Played);
			return result;
		}
	}

	[SerializeField]
	private UnitAnimationAction m_Action;

	protected override void OnActivateOrPostLoad()
	{
		TryPlay();
	}

	protected override void OnViewDidAttach()
	{
		TryPlay();
	}

	private void TryPlay()
	{
		if (m_Action == null)
		{
			return;
		}
		SavableData savableData = RequestSavableData<SavableData>();
		if (!savableData.Played)
		{
			UnitAnimationManager unitAnimationManager = base.Owner.View.Or(null)?.AnimationManager;
			if (!(unitAnimationManager == null))
			{
				unitAnimationManager.Execute(unitAnimationManager.CreateHandle(m_Action));
				savableData.Played = true;
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
