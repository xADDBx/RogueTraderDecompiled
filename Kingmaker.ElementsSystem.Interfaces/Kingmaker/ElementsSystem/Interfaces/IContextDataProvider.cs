using System;

namespace Kingmaker.ElementsSystem.Interfaces;

public interface IContextDataProvider
{
	IDisposable RequestContextData();
}
