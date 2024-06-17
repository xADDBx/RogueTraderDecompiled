using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[TypeId("a0873ec724b2442881717f9977245847")]
public class RequirementScrap : Requirement
{
	public int Scrap;

	public override bool Check(Colony colony = null)
	{
		return Scrap <= (int)Game.Instance.Player.Scrap;
	}

	public override void Apply(Colony colony = null)
	{
		Game.Instance.Player.Scrap.Spend(Scrap);
	}
}
