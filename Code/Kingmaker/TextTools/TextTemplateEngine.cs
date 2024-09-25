using System.Linq;
using Kingmaker.TextTools.Base;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Common;

namespace Kingmaker.TextTools;

public class TextTemplateEngine : BaseTextTemplateEngine
{
	private static readonly TextTemplateEngine s_Instance = new TextTemplateEngine();

	public static TextTemplateEngine Instance => s_Instance;

	private TextTemplateEngine()
	{
		AddTemplates();
	}

	private void AddTemplates()
	{
		AddTemplate("mf", new MaleFemaleTemplate());
		AddTemplate("rt_mf", new RtMaleFemaleTemplate());
		AddTemplate("race", new RaceTemplate());
		AddTemplate("name", new NameTemplate());
		AddTemplate("rt_name", new RtNameTemplate());
		AddTemplate("date", new DateTemplate());
		AddTemplate("time", new TimeTempate());
		AddTemplate("custom_companion_cost", new CustomCompanionCostTemplate());
		AddTemplate("respec_cost", new RespecCostTemplate());
		AddTemplate("n", new NarratorStartTemplate());
		AddTemplate("/n", new NarratorEndTemplate());
		AddTemplate("g", new TooltipStartTemplate(TooltipType.Glosary));
		AddTemplate("/g", new TooltipEndTemplate(TooltipType.Glosary));
		AddTemplate("d", new TooltipStartTemplate(TooltipType.Decisions));
		AddTemplate("/d", new TooltipEndTemplate(TooltipType.Decisions));
		AddTemplate("m", new TooltipStartTemplate(TooltipType.Mechanics));
		AddTemplate("/m", new TooltipEndTemplate(TooltipType.Mechanics));
		AddTemplate("target", new LogTemplateTarget());
		AddTemplate("formula", new LogTemplateFormula());
		AddTemplate("source", new LogTemplateSource());
		AddTemplate("text", new LogTemplateText());
		AddTemplate("second_text", new LogTemplateSecondText());
		AddTemplate("description", new LogTemplateDescription());
		AddTemplate("count", new LogTemplateCount());
		AddTemplate("count_form", new LogTemplateCountForm());
		AddTemplate("roll", new LogTemplateRoll());
		AddTemplate("d10", GameLogTemplates.D10);
		AddTemplate("d100", GameLogTemplates.D100);
		AddTemplate("hit.d100", GameLogTemplates.HitD100);
		AddTemplate("dodge.d100", GameLogTemplates.DodgeD100);
		AddTemplate("parry.d100", GameLogTemplates.ParryD100);
		AddTemplate("rf.d100", GameLogTemplates.RfD100);
		AddTemplate("cover_hit.d100", GameLogTemplates.CoverHitD100);
		AddTemplate("hit.chance", GameLogTemplates.HitChance);
		AddTemplate("dodge.chance", GameLogTemplates.DodgeChance);
		AddTemplate("parry.chance", GameLogTemplates.ParryChance);
		AddTemplate("rf.chance", GameLogTemplates.RfChance);
		AddTemplate("cover_hit.chance", GameLogTemplates.CoverHitChance);
		AddTemplate("total_hit.chance", GameLogTemplates.TotalHitChance);
		AddTemplate("target_superiority_penalty", GameLogTemplates.TargetSuperiorityPenalty);
		AddTemplate("pre_mitigation_damage", GameLogTemplates.PreMitigationDamage);
		AddTemplate("penetration", GameLogTemplates.Penetration);
		AddTemplate("absorption", GameLogTemplates.Absorption);
		AddTemplate("deflection", GameLogTemplates.Deflection);
		AddTemplate("absorption_with_penetration", GameLogTemplates.AbsorptionWithPenetration);
		AddTemplate("result_damage", GameLogTemplates.ResultDamage);
		AddTemplate("damage.difficulty_modifier", GameLogTemplates.DifficultyModifier);
		AddTemplate("mod", new LogTemplateModifier());
		AddTemplate("dc", new LogTemplateDC());
		AddTemplate("chance_dc", new LogTemplateChanceDC());
		AddTemplate("rations", new UITemplateRations());
		AddTemplate("recipe", new UITemplateSimpleText());
		AddTemplate("attack_number", new LogTemplateAttackNumber());
		AddTemplate("attacks_count", new LogTemplateAttacksCount());
		AddTemplate("round", new LogTemplateRound());
		AddTemplate("portraits_path", new UITemplatePartraitsPath());
		AddTemplate("area_name", new UITemplateAreaName());
		AddTemplate("unit_stat", new UnitStatStartTemplate());
		AddTemplate("armour.dodge", new UITemplateArmourDodge());
		AddTemplate("armour.damage_reduce", new UITemplateArmourDamageReduce());
		AddTemplate("bind", new KeyBindingTemplate());
		AddTemplate("console_bind", new ConsoleBindingTemplate());
		AddTemplate("empty", new EmptyTemplate());
		AddTemplate("br", new LineBreakTemplate());
		AddTemplate("pc_console", new PcConsoleTemplate());
		AddTemplate("t", new TutorialDataTemplate());
		AddTemplate("ui", new GlossaryTemplate());
		AddTemplate("gd", new GlossaryTemplate());
		AddTemplate("veil", new VeilTemplate());
		AddTemplate("veil_delta_value", new VeilDeltaValue());
		AddTemplate("veil_value", new VeilValue());
		AddTemplate("momentum", new MomentumTemplate());
		AddTemplate("momentum_delta_value", new MomentumDeltaValue());
		AddTemplate("momentum_value", new MomentumValue());
		AddTemplate("push", new PushTemplate());
		AddTemplate("overpenetration", new OverpenetrationTemplate());
		AddTemplate("critical_hit", new CriticalHitTemplate());
		AddTemplate("damage.type", new DamageTypeTemplate());
	}

	public static string[] GetTemplateTags()
	{
		return BaseTextTemplateEngine.TemplatesByTag.Keys.ToArray();
	}
}
