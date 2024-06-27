using System;

namespace Kingmaker.ElementsSystem.ContextData;

public abstract class ContextScope<TScope, TContext> : ContextData<TScope> where TScope : ContextScope<TScope, TContext>, new()
{
	public TContext Context { get; private set; }

	public static TScope Enter(TContext context)
	{
		TScope val = ContextData<TScope>.Request();
		val.Context = context;
		try
		{
			val.OnEnter();
		}
		catch (Exception ex)
		{
			ContextData.LogChannel.Exception(ex);
		}
		return val;
	}

	protected sealed override void Reset()
	{
		try
		{
			OnExit();
		}
		catch (Exception ex)
		{
			ContextData.LogChannel.Exception(ex);
		}
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnExit()
	{
	}
}
