using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UnitLogic.Buffs.Actions;

[TypeId("15072199979bf174da1112d62b863644")]
public class BuffActionAddStatBonus : BuffAction
{
	public StatType Stat;

	public ContextValue Value;

	public ModifierDescriptor Descriptor;

	public override string GetCaption()
	{
		return "BuffActionAddStatBonus, invalid, do nothing";
	}

	protected override void RunAction()
	{
	}
}
