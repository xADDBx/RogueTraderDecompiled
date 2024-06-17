using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kingmaker.Utility.Fsm;

public abstract class StateLongAsync : IStateAsync
{
	private CancellationTokenSource m_OperationCts;

	private Task m_OperationTask = Task.CompletedTask;

	public virtual Task OnEnter()
	{
		RunAction();
		return Task.CompletedTask;
	}

	public virtual async Task OnExit()
	{
		if (!m_OperationTask.IsCompleted)
		{
			try
			{
				m_OperationCts.Cancel();
				await m_OperationTask;
			}
			catch (OperationCanceledException)
			{
			}
		}
	}

	protected abstract Task DoAction(CancellationToken cancellationToken);

	private async void RunAction()
	{
		try
		{
			m_OperationCts = new CancellationTokenSource();
			m_OperationTask = DoAction(m_OperationCts.Token);
			await m_OperationTask;
		}
		catch (OperationCanceledException)
		{
			OnActionCancelled();
		}
		catch (Exception exception)
		{
			OnActionException(exception);
		}
		finally
		{
			m_OperationCts.Dispose();
			m_OperationCts = null;
		}
	}

	protected virtual void OnActionException(Exception exception)
	{
	}

	protected virtual void OnActionCancelled()
	{
	}
}
