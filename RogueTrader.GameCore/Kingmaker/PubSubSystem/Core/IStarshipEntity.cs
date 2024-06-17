using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IStarshipEntity : IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable
{
}
