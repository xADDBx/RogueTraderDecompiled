using System;

namespace Owlcat.Runtime.UI.MVVM;

public interface IBaseDisposable : IDisposable
{
	event Action OnDispose;
}
