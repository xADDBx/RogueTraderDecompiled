using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IStarSystemObjectEntity : IMapObjectEntity, IMechanicEntity, IEntity, IDisposable
{
}
