using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cci.Pdb;

internal class PdbFunction
{
	internal class PdbFunctionsByAddress : IComparer
	{
		public int Compare(object x, object y)
		{
			PdbFunction pdbFunction = (PdbFunction)x;
			PdbFunction pdbFunction2 = (PdbFunction)y;
			if (pdbFunction.segment < pdbFunction2.segment)
			{
				return -1;
			}
			if (pdbFunction.segment > pdbFunction2.segment)
			{
				return 1;
			}
			if (pdbFunction.address < pdbFunction2.address)
			{
				return -1;
			}
			if (pdbFunction.address > pdbFunction2.address)
			{
				return 1;
			}
			return 0;
		}
	}

	internal class PdbFunctionsByAddressAndToken : IComparer
	{
		public int Compare(object x, object y)
		{
			PdbFunction pdbFunction = (PdbFunction)x;
			PdbFunction pdbFunction2 = (PdbFunction)y;
			if (pdbFunction.segment < pdbFunction2.segment)
			{
				return -1;
			}
			if (pdbFunction.segment > pdbFunction2.segment)
			{
				return 1;
			}
			if (pdbFunction.address < pdbFunction2.address)
			{
				return -1;
			}
			if (pdbFunction.address > pdbFunction2.address)
			{
				return 1;
			}
			if (pdbFunction.token < pdbFunction2.token)
			{
				return -1;
			}
			if (pdbFunction.token > pdbFunction2.token)
			{
				return 1;
			}
			return 0;
		}
	}

	internal static readonly Guid msilMetaData = new Guid(3337240521u, 22963, 18902, 188, 37, 9, 2, 187, 171, 180, 96);

	internal static readonly IComparer byAddress = new PdbFunctionsByAddress();

	internal static readonly IComparer byAddressAndToken = new PdbFunctionsByAddressAndToken();

	internal uint token;

	internal uint slotToken;

	internal uint tokenOfMethodWhoseUsingInfoAppliesToThisMethod;

	internal uint segment;

	internal uint address;

	internal uint length;

	internal PdbScope[] scopes;

	internal PdbSlot[] slots;

	internal PdbConstant[] constants;

	internal string[] usedNamespaces;

	internal PdbLines[] lines;

	internal ushort[] usingCounts;

	internal IEnumerable<INamespaceScope> namespaceScopes;

	internal string iteratorClass;

	internal List<ILocalScope> iteratorScopes;

	internal PdbSynchronizationInformation synchronizationInformation;

	private bool visualBasicScopesAdjusted;

	private static string StripNamespace(string module)
	{
		int num = module.LastIndexOf('.');
		if (num > 0)
		{
			return module.Substring(num + 1);
		}
		return module;
	}

	internal void AdjustVisualBasicScopes()
	{
		if (!visualBasicScopesAdjusted)
		{
			visualBasicScopesAdjusted = true;
			PdbScope[] array = scopes;
			foreach (PdbScope pdbScope in array)
			{
				AdjustVisualBasicScopes(pdbScope.scopes);
			}
		}
	}

	private void AdjustVisualBasicScopes(PdbScope[] scopes)
	{
		foreach (PdbScope pdbScope in scopes)
		{
			pdbScope.length++;
			AdjustVisualBasicScopes(pdbScope.scopes);
		}
	}

	internal static PdbFunction[] LoadManagedFunctions(BitAccess bits, uint limit, bool readStrings)
	{
		int position = bits.Position;
		int num = 0;
		ManProcSym manProcSym = default(ManProcSym);
		while (bits.Position < limit)
		{
			bits.ReadUInt16(out var value);
			int position2 = bits.Position;
			int position3 = bits.Position + value;
			bits.Position = position2;
			bits.ReadUInt16(out var value2);
			switch ((SYM)value2)
			{
			case SYM.S_GMANPROC:
			case SYM.S_LMANPROC:
				bits.ReadUInt32(out manProcSym.parent);
				bits.ReadUInt32(out manProcSym.end);
				bits.Position = (int)manProcSym.end;
				num++;
				break;
			case SYM.S_END:
				bits.Position = position3;
				break;
			default:
				bits.Position = position3;
				break;
			}
		}
		if (num == 0)
		{
			return null;
		}
		bits.Position = position;
		PdbFunction[] array = new PdbFunction[num];
		int num2 = 0;
		ManProcSym proc = default(ManProcSym);
		while (bits.Position < limit)
		{
			bits.ReadUInt16(out var value3);
			_ = bits.Position;
			int position4 = bits.Position + value3;
			bits.ReadUInt16(out var value4);
			SYM sYM = (SYM)value4;
			if ((uint)(sYM - 4394) <= 1u)
			{
				bits.ReadUInt32(out proc.parent);
				bits.ReadUInt32(out proc.end);
				bits.ReadUInt32(out proc.next);
				bits.ReadUInt32(out proc.len);
				bits.ReadUInt32(out proc.dbgStart);
				bits.ReadUInt32(out proc.dbgEnd);
				bits.ReadUInt32(out proc.token);
				bits.ReadUInt32(out proc.off);
				bits.ReadUInt16(out proc.seg);
				bits.ReadUInt8(out proc.flags);
				bits.ReadUInt16(out proc.retReg);
				if (readStrings)
				{
					bits.ReadCString(out proc.name);
				}
				else
				{
					bits.SkipCString(out proc.name);
				}
				bits.Position = position4;
				array[num2++] = new PdbFunction(proc, bits);
			}
			else
			{
				bits.Position = position4;
			}
		}
		return array;
	}

	internal static void CountScopesAndSlots(BitAccess bits, uint limit, out int constants, out int scopes, out int slots, out int usedNamespaces)
	{
		int position = bits.Position;
		constants = 0;
		slots = 0;
		scopes = 0;
		usedNamespaces = 0;
		BlockSym32 blockSym = default(BlockSym32);
		while (bits.Position < limit)
		{
			bits.ReadUInt16(out var value);
			int position2 = bits.Position;
			int position3 = bits.Position + value;
			bits.Position = position2;
			bits.ReadUInt16(out var value2);
			switch ((SYM)value2)
			{
			case SYM.S_BLOCK32:
				bits.ReadUInt32(out blockSym.parent);
				bits.ReadUInt32(out blockSym.end);
				scopes++;
				bits.Position = (int)blockSym.end;
				break;
			case SYM.S_MANSLOT:
				slots++;
				bits.Position = position3;
				break;
			case SYM.S_UNAMESPACE:
				usedNamespaces++;
				bits.Position = position3;
				break;
			case SYM.S_MANCONSTANT:
				constants++;
				bits.Position = position3;
				break;
			default:
				bits.Position = position3;
				break;
			}
		}
		bits.Position = position;
	}

	internal PdbFunction()
	{
	}

	internal PdbFunction(ManProcSym proc, BitAccess bits)
	{
		token = proc.token;
		segment = proc.seg;
		address = proc.off;
		length = proc.len;
		if (proc.seg != 1)
		{
			throw new PdbDebugException("Segment is {0}, not 1.", proc.seg);
		}
		if (proc.parent != 0 || proc.next != 0)
		{
			throw new PdbDebugException("Warning parent={0}, next={1}", proc.parent, proc.next);
		}
		CountScopesAndSlots(bits, proc.end, out var num, out var num2, out var num3, out var num4);
		int num5 = ((num > 0 || num3 > 0 || num4 > 0) ? 1 : 0);
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		scopes = new PdbScope[num2 + num5];
		slots = new PdbSlot[num3];
		constants = new PdbConstant[num];
		usedNamespaces = new string[num4];
		if (num5 > 0)
		{
			scopes[0] = new PdbScope(address, proc.len, slots, constants, usedNamespaces);
		}
		OemSymbol oemSymbol = default(OemSymbol);
		while (bits.Position < proc.end)
		{
			bits.ReadUInt16(out var value);
			int position = bits.Position;
			int position2 = bits.Position + value;
			bits.Position = position;
			bits.ReadUInt16(out var value2);
			switch ((SYM)value2)
			{
			case SYM.S_OEM:
				bits.ReadGuid(out oemSymbol.idOem);
				bits.ReadUInt32(out oemSymbol.typind);
				if (oemSymbol.idOem == msilMetaData)
				{
					string text = bits.ReadString();
					if (text == "MD2")
					{
						ReadMD2CustomMetadata(bits);
					}
					else if (text == "asyncMethodInfo")
					{
						synchronizationInformation = new PdbSynchronizationInformation(bits);
					}
					bits.Position = position2;
					break;
				}
				throw new PdbDebugException("OEM section: guid={0} ti={1}", oemSymbol.idOem, oemSymbol.typind);
			case SYM.S_BLOCK32:
			{
				BlockSym32 block = default(BlockSym32);
				bits.ReadUInt32(out block.parent);
				bits.ReadUInt32(out block.end);
				bits.ReadUInt32(out block.len);
				bits.ReadUInt32(out block.off);
				bits.ReadUInt16(out block.seg);
				bits.SkipCString(out block.name);
				bits.Position = position2;
				scopes[num5++] = new PdbScope(address, block, bits, out slotToken);
				bits.Position = (int)block.end;
				break;
			}
			case SYM.S_MANSLOT:
				slots[num6++] = new PdbSlot(bits);
				bits.Position = position2;
				break;
			case SYM.S_MANCONSTANT:
				constants[num7++] = new PdbConstant(bits);
				bits.Position = position2;
				break;
			case SYM.S_UNAMESPACE:
				bits.ReadCString(out usedNamespaces[num8++]);
				bits.Position = position2;
				break;
			case SYM.S_END:
				bits.Position = position2;
				break;
			default:
				bits.Position = position2;
				break;
			}
		}
		if (bits.Position != proc.end)
		{
			throw new PdbDebugException("Not at S_END");
		}
		bits.ReadUInt16(out var _);
		bits.ReadUInt16(out var value4);
		if (value4 != 6)
		{
			throw new PdbDebugException("Missing S_END");
		}
	}

	internal void ReadMD2CustomMetadata(BitAccess bits)
	{
		bits.ReadUInt8(out var value);
		if (value == 4)
		{
			bits.ReadUInt8(out var value2);
			bits.Align(4);
			while (value2-- > 0)
			{
				ReadCustomMetadata(bits);
			}
		}
	}

	private void ReadCustomMetadata(BitAccess bits)
	{
		int position = bits.Position;
		bits.ReadUInt8(out var value);
		bits.ReadUInt8(out var value2);
		bits.Position += 2;
		bits.ReadUInt32(out var value3);
		if (value == 4)
		{
			switch (value2)
			{
			case 0:
				ReadUsingInfo(bits);
				break;
			case 1:
				ReadForwardInfo(bits);
				break;
			case 3:
				ReadIteratorLocals(bits);
				break;
			case 4:
				ReadForwardIterator(bits);
				break;
			}
		}
		bits.Position = position + (int)value3;
	}

	private void ReadForwardIterator(BitAccess bits)
	{
		iteratorClass = bits.ReadString();
	}

	private void ReadIteratorLocals(BitAccess bits)
	{
		bits.ReadUInt32(out var value);
		iteratorScopes = new List<ILocalScope>((int)value);
		while (value-- != 0)
		{
			bits.ReadUInt32(out var value2);
			bits.ReadUInt32(out var value3);
			iteratorScopes.Add(new PdbIteratorScope(value2, value3 - value2));
		}
	}

	private void ReadForwardInfo(BitAccess bits)
	{
		bits.ReadUInt32(out tokenOfMethodWhoseUsingInfoAppliesToThisMethod);
	}

	private void ReadUsingInfo(BitAccess bits)
	{
		bits.ReadUInt16(out var value);
		usingCounts = new ushort[value];
		for (ushort num = 0; num < value; num++)
		{
			bits.ReadUInt16(out usingCounts[num]);
		}
	}
}
