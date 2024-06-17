using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Progression.Prerequisites;

namespace Kingmaker.PubSubSystem.Core;

public interface IBaseUnitEntity : IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable
{
	bool MeetsPrerequisite(PrerequisiteStat stat);
}
