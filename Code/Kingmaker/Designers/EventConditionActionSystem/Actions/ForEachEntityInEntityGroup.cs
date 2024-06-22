using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("680a226cd91444998ed9d98f3b6cb393")]
public class ForEachEntityInEntityGroup : GameAction
{
	[SerializeField]
	[AllowedEntityType(typeof(EntityGroupView))]
	private EntityReference m_Group;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Do Action for each Entity in Group";
	}

	protected override void RunAction()
	{
		IEntityViewBase entityViewBase = m_Group.FindView();
		if (entityViewBase == null)
		{
			return;
		}
		foreach (Transform item in entityViewBase.ViewTransform)
		{
			EntityViewBase component = item.GetComponent<EntityViewBase>();
			if (component == null)
			{
				continue;
			}
			MechanicEntity mechanicEntity = (MechanicEntity)component.Data;
			if (mechanicEntity != null)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(mechanicEntity))
				{
					Actions.Run();
				}
			}
		}
	}
}
