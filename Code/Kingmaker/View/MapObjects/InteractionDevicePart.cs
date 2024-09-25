using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionDevicePart : InteractionPart<InteractionDeviceSettings>, IHashable
{
	private Animator m_Animator;

	[JsonProperty]
	public int State;

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		m_Animator = base.View.Or(null)?.GetComponentInChildren<Animator>();
		m_Animator.Or(null)?.SetInteger("State", State);
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		int value = (State = SelectNextState());
		m_Animator.SetInteger("State", value);
	}

	private int SelectNextState()
	{
		int num = State + 1;
		if (num >= base.Settings.StatesCount)
		{
			num = 0;
		}
		return num;
	}

	public void SetState(int state)
	{
		State = state;
		m_Animator.SetInteger("State", state);
	}

	public void SetTrigger(string trigger)
	{
		m_Animator.SetTrigger(trigger);
	}

	public int GetState()
	{
		return State;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref State);
		return result;
	}
}
