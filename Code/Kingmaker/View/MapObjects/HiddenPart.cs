using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class HiddenPart : InteractionPart<HiddenSettings>, IHashable
{
	[JsonProperty]
	public bool Checked { get; private set; }

	[JsonProperty]
	public bool Opened { get; private set; }

	public StatType StatType => base.Settings.StatType;

	public int DC => base.Settings.DifficultyClass;

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		SetHidden(!Opened);
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (!Checked)
		{
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, base.Settings.StatType, base.Settings.DifficultyClass)
			{
				Voice = RulePerformSkillCheck.VoicingType.Success
			});
			Checked = true;
			Opened = rulePerformSkillCheck.ResultIsSuccess;
			SetHidden(!Opened);
		}
	}

	private void SetHidden(bool hidden)
	{
		foreach (InteractionPart item in base.Owner.Parts.GetAll<InteractionPart>())
		{
			if (item != this)
			{
				item.Enabled = !hidden;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Checked;
		result.Append(ref val2);
		bool val3 = Opened;
		result.Append(ref val3);
		return result;
	}
}
