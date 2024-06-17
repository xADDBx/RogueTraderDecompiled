using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public static class GameLogTemplates
{
	public static readonly TextTemplate D10 = new LogTemplateDice<IRuleRollD10>(() => GameLogContext.D10.Value);

	public static readonly TextTemplate D100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.D100.Value);

	public static readonly TextTemplate HitD100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.HitD100.Value);

	public static readonly TextTemplate DodgeD100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.DodgeD100.Value);

	public static readonly TextTemplate ParryD100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.ParryD100.Value);

	public static readonly TextTemplate RfD100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.RfD100.Value);

	public static readonly TextTemplate CoverHitD100 = new LogTemplateDice<IRuleRollD100>(() => GameLogContext.CoverHitD100.Value);

	public static readonly TextTemplate HitChance = new LogTemplateTrivial<int>(() => GameLogContext.HitChance);

	public static readonly TextTemplate DodgeChance = new LogTemplateTrivial<int>(() => GameLogContext.DodgeChance);

	public static readonly TextTemplate ParryChance = new LogTemplateTrivial<int>(() => GameLogContext.ParryChance);

	public static readonly TextTemplate RfChance = new LogTemplateTrivial<int>(() => GameLogContext.RfChance);

	public static readonly TextTemplate CoverHitChance = new LogTemplateTrivial<int>(() => GameLogContext.CoverHitChance);

	public static readonly TextTemplate TotalHitChance = new LogTemplateTrivial<int>(() => GameLogContext.TotalHitChance);

	public static readonly TextTemplate TargetSuperiorityPenalty = new LogTemplateTrivial<int>(() => GameLogContext.TargetSuperiorityPenalty);

	public static readonly TextTemplate PreMitigationDamage = new LogTemplateTrivial<int>(() => GameLogContext.PreMitigationDamage);

	public static readonly TextTemplate Absorption = new LogTemplateTrivial<int>(() => GameLogContext.Absorption);

	public static readonly TextTemplate Deflection = new LogTemplateTrivial<int>(() => GameLogContext.Deflection);

	public static readonly TextTemplate Penetration = new LogTemplateTrivial<int>(() => GameLogContext.Penetration);

	public static readonly TextTemplate AbsorptionWithPenetration = new LogTemplateTrivial<int>(() => GameLogContext.AbsorptionWithPenetration);

	public static readonly TextTemplate ResultDamage = new LogTemplateTrivial<int>(() => GameLogContext.ResultDamage);

	public static readonly TextTemplate DifficultyModifier = new LogTemplateTrivial<float>(() => (int)GameLogContext.DifficultyModifier);
}
