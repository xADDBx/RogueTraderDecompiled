using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitEntity : IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable
{
}
