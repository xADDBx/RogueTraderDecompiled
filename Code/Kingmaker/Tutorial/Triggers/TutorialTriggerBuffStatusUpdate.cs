using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Tags: 't|SourceFact' - Triggered buff\n't|TargetUnit' - Unit who acquired buff")]
[TypeId("990500828f6b1c54ca434d39037fb36e")]
public class TutorialTriggerBuffStatusUpdate : TutorialTrigger, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	private enum BuffEventType
	{
		Attach,
		Detach
	}

	[SerializeField]
	private BuffEventType m_EventType;

	[SerializeField]
	private bool m_ChooseSpecificBuff = true;

	[SerializeField]
	[ShowIf("m_ChooseSpecificBuff")]
	private BlueprintBuffReference m_Buff;

	[HideIf("m_ChooseSpecificBuff")]
	public SpellDescriptorWrapper TriggerDescriptors;

	[HideIf("m_ChooseSpecificBuff")]
	[InfoBox(Text = "If NeedAllDescriptors is true, only buff that has all listed flags will trigger")]
	public bool NeedAllDescriptors;

	public void HandleBuffDidAdded(Buff buff)
	{
		if (m_EventType == BuffEventType.Attach)
		{
			TryToTrigger(buff);
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (m_EventType == BuffEventType.Detach)
		{
			TryToTrigger(buff);
		}
	}

	private void TryToTrigger(Buff buff)
	{
		if (buff.Owner.Faction.IsPlayer && buff.Owner.IsInCompanionRoster() && (!m_ChooseSpecificBuff || m_Buff != null) && !((!m_ChooseSpecificBuff && NeedAllDescriptors) ? (!buff.Context.SpellDescriptor.HasAllFlags(TriggerDescriptors.Value)) : (!buff.Context.SpellDescriptor.HasAnyFlag(TriggerDescriptors.Value))))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceFact = buff;
				context.TargetUnit = buff.Owner;
			});
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
