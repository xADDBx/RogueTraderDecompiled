using System;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class AsyncMethodBodyDebugInformation : CustomDebugInformation
{
	internal InstructionOffset catch_handler;

	internal Collection<InstructionOffset> yields;

	internal Collection<InstructionOffset> resumes;

	internal Collection<MethodDefinition> resume_methods;

	public static Guid KindIdentifier = new Guid("{54FD2AC5-E925-401A-9C2A-F94F171072F8}");

	public InstructionOffset CatchHandler
	{
		get
		{
			return catch_handler;
		}
		set
		{
			catch_handler = value;
		}
	}

	public Collection<InstructionOffset> Yields
	{
		get
		{
			if (yields == null)
			{
				Interlocked.CompareExchange(ref yields, new Collection<InstructionOffset>(), null);
			}
			return yields;
		}
	}

	public Collection<InstructionOffset> Resumes
	{
		get
		{
			if (resumes == null)
			{
				Interlocked.CompareExchange(ref resumes, new Collection<InstructionOffset>(), null);
			}
			return resumes;
		}
	}

	public Collection<MethodDefinition> ResumeMethods => resume_methods ?? (resume_methods = new Collection<MethodDefinition>());

	public override CustomDebugInformationKind Kind => CustomDebugInformationKind.AsyncMethodBody;

	internal AsyncMethodBodyDebugInformation(int catchHandler)
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(catchHandler);
	}

	public AsyncMethodBodyDebugInformation(Instruction catchHandler)
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(catchHandler);
	}

	public AsyncMethodBodyDebugInformation()
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(-1);
	}
}
