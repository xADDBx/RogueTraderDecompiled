using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Cci.Pdb;

internal class PdbFile
{
	private static readonly Guid BasicLanguageGuid = new Guid(974311608, -15764, 4560, 180, 66, 0, 160, 36, 74, 29, 210);

	public static readonly Guid SymDocumentType_Text = new Guid(1518771467, 26129, 4563, 189, 42, 0, 0, 248, 8, 73, 189);

	private PdbFile()
	{
	}

	private static void LoadInjectedSourceInformation(BitAccess bits, out Guid doctype, out Guid language, out Guid vendor, out Guid checksumAlgo, out byte[] checksum)
	{
		checksum = null;
		bits.ReadGuid(out language);
		bits.ReadGuid(out vendor);
		bits.ReadGuid(out doctype);
		bits.ReadGuid(out checksumAlgo);
		bits.ReadInt32(out var value);
		bits.ReadInt32(out var _);
		if (value > 0)
		{
			checksum = new byte[value];
			bits.ReadBytes(checksum);
		}
	}

	private static Dictionary<string, int> LoadNameIndex(BitAccess bits, out int age, out Guid guid)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		bits.ReadInt32(out var _);
		bits.ReadInt32(out var _);
		bits.ReadInt32(out age);
		bits.ReadGuid(out guid);
		bits.ReadInt32(out var value3);
		int position = bits.Position;
		int position2 = bits.Position + value3;
		bits.Position = position2;
		bits.ReadInt32(out var value4);
		bits.ReadInt32(out var value5);
		BitSet bitSet = new BitSet(bits);
		new BitSet(bits);
		int num = 0;
		for (int i = 0; i < value5; i++)
		{
			if (bitSet.IsSet(i))
			{
				bits.ReadInt32(out var value6);
				bits.ReadInt32(out var value7);
				int position3 = bits.Position;
				bits.Position = position + value6;
				bits.ReadCString(out var value8);
				bits.Position = position3;
				dictionary.Add(value8.ToUpperInvariant(), value7);
				num++;
			}
		}
		if (num != value4)
		{
			throw new PdbDebugException("Count mismatch. ({0} != {1})", num, value4);
		}
		return dictionary;
	}

	private static IntHashTable LoadNameStream(BitAccess bits)
	{
		IntHashTable intHashTable = new IntHashTable();
		bits.ReadUInt32(out var value);
		bits.ReadInt32(out var value2);
		bits.ReadInt32(out var value3);
		if (value != 4026462206u || value2 != 1)
		{
			throw new PdbDebugException("Unsupported Name Stream version. (sig={0:x8}, ver={1})", value, value2);
		}
		int position = bits.Position;
		int position2 = bits.Position + value3;
		bits.Position = position2;
		bits.ReadInt32(out var value4);
		position2 = bits.Position;
		for (int i = 0; i < value4; i++)
		{
			bits.ReadInt32(out var value5);
			if (value5 != 0)
			{
				int position3 = bits.Position;
				bits.Position = position + value5;
				bits.ReadCString(out var value6);
				bits.Position = position3;
				intHashTable.Add(value5, value6);
			}
		}
		bits.Position = position2;
		return intHashTable;
	}

	private static int FindFunction(PdbFunction[] funcs, ushort sec, uint off)
	{
		PdbFunction value = new PdbFunction
		{
			segment = sec,
			address = off
		};
		return Array.BinarySearch(funcs, value, PdbFunction.byAddress);
	}

	private static void LoadManagedLines(PdbFunction[] funcs, IntHashTable names, BitAccess bits, MsfDirectory dir, Dictionary<string, int> nameIndex, PdbReader reader, uint limit, Dictionary<string, PdbSource> sourceCache)
	{
		Array.Sort(funcs, PdbFunction.byAddressAndToken);
		int position = bits.Position;
		IntHashTable intHashTable = ReadSourceFileInfo(bits, limit, names, dir, nameIndex, reader, sourceCache);
		bits.Position = position;
		CV_LineSection cV_LineSection = default(CV_LineSection);
		CV_SourceFile cV_SourceFile = default(CV_SourceFile);
		CV_SourceFile cV_SourceFile2 = default(CV_SourceFile);
		CV_Line cV_Line = default(CV_Line);
		while (bits.Position < limit)
		{
			bits.ReadInt32(out var value);
			bits.ReadInt32(out var value2);
			int num = bits.Position + value2;
			if (value == 242)
			{
				bits.ReadUInt32(out cV_LineSection.off);
				bits.ReadUInt16(out cV_LineSection.sec);
				bits.ReadUInt16(out cV_LineSection.flags);
				bits.ReadUInt32(out cV_LineSection.cod);
				int i = FindFunction(funcs, cV_LineSection.sec, cV_LineSection.off);
				if (i >= 0)
				{
					PdbFunction pdbFunction = funcs[i];
					if (pdbFunction.lines == null)
					{
						while (i > 0)
						{
							PdbFunction pdbFunction2 = funcs[i - 1];
							if (pdbFunction2.lines != null || pdbFunction2.segment != cV_LineSection.sec || pdbFunction2.address != cV_LineSection.off)
							{
								break;
							}
							pdbFunction = pdbFunction2;
							i--;
						}
					}
					else
					{
						for (; i < funcs.Length - 1; i++)
						{
							if (pdbFunction.lines == null)
							{
								break;
							}
							PdbFunction pdbFunction3 = funcs[i + 1];
							if (pdbFunction3.segment != cV_LineSection.sec || pdbFunction3.address != cV_LineSection.off)
							{
								break;
							}
							pdbFunction = pdbFunction3;
						}
					}
					if (pdbFunction.lines == null)
					{
						int position2 = bits.Position;
						int num2 = 0;
						while (bits.Position < num)
						{
							bits.ReadUInt32(out cV_SourceFile.index);
							bits.ReadUInt32(out cV_SourceFile.count);
							bits.ReadUInt32(out cV_SourceFile.linsiz);
							int num3 = (int)cV_SourceFile.count * (8 + ((((uint)cV_LineSection.flags & (true ? 1u : 0u)) != 0) ? 4 : 0));
							bits.Position += num3;
							num2++;
						}
						pdbFunction.lines = new PdbLines[num2];
						int num4 = 0;
						bits.Position = position2;
						while (bits.Position < num)
						{
							bits.ReadUInt32(out cV_SourceFile2.index);
							bits.ReadUInt32(out cV_SourceFile2.count);
							bits.ReadUInt32(out cV_SourceFile2.linsiz);
							PdbSource obj = (PdbSource)intHashTable[(int)cV_SourceFile2.index];
							if (obj.language.Equals(BasicLanguageGuid))
							{
								pdbFunction.AdjustVisualBasicScopes();
							}
							PdbLines pdbLines = new PdbLines(obj, cV_SourceFile2.count);
							pdbFunction.lines[num4++] = pdbLines;
							PdbLine[] lines = pdbLines.lines;
							int position3 = bits.Position;
							int num5 = bits.Position + (int)(8 * cV_SourceFile2.count);
							for (int j = 0; j < cV_SourceFile2.count; j++)
							{
								CV_Column cV_Column = default(CV_Column);
								bits.Position = position3 + 8 * j;
								bits.ReadUInt32(out cV_Line.offset);
								bits.ReadUInt32(out cV_Line.flags);
								uint num6 = cV_Line.flags & 0xFFFFFFu;
								uint num7 = (cV_Line.flags & 0x7F000000) >> 24;
								if (((uint)cV_LineSection.flags & (true ? 1u : 0u)) != 0)
								{
									bits.Position = num5 + 4 * j;
									bits.ReadUInt16(out cV_Column.offColumnStart);
									bits.ReadUInt16(out cV_Column.offColumnEnd);
								}
								lines[j] = new PdbLine(cV_Line.offset, num6, cV_Column.offColumnStart, num6 + num7, cV_Column.offColumnEnd);
							}
						}
					}
				}
			}
			bits.Position = num;
		}
	}

	private static void LoadFuncsFromDbiModule(BitAccess bits, DbiModuleInfo info, IntHashTable names, List<PdbFunction> funcList, bool readStrings, MsfDirectory dir, Dictionary<string, int> nameIndex, PdbReader reader, Dictionary<string, PdbSource> sourceCache)
	{
		PdbFunction[] array = null;
		bits.Position = 0;
		bits.ReadInt32(out var value);
		if (value != 4)
		{
			throw new PdbDebugException("Invalid signature. (sig={0})", value);
		}
		bits.Position = 4;
		array = PdbFunction.LoadManagedFunctions(bits, (uint)info.cbSyms, readStrings);
		if (array != null)
		{
			bits.Position = info.cbSyms + info.cbOldLines;
			LoadManagedLines(array, names, bits, dir, nameIndex, reader, (uint)(info.cbSyms + info.cbOldLines + info.cbLines), sourceCache);
			for (int i = 0; i < array.Length; i++)
			{
				funcList.Add(array[i]);
			}
		}
	}

	private static void LoadDbiStream(BitAccess bits, out DbiModuleInfo[] modules, out DbiDbgHdr header, bool readStrings)
	{
		DbiHeader dbiHeader = new DbiHeader(bits);
		header = default(DbiDbgHdr);
		List<DbiModuleInfo> list = new List<DbiModuleInfo>();
		int num = bits.Position + dbiHeader.gpmodiSize;
		while (bits.Position < num)
		{
			DbiModuleInfo item = new DbiModuleInfo(bits, readStrings);
			list.Add(item);
		}
		if (bits.Position != num)
		{
			throw new PdbDebugException("Error reading DBI stream, pos={0} != {1}", bits.Position, num);
		}
		if (list.Count > 0)
		{
			modules = list.ToArray();
		}
		else
		{
			modules = null;
		}
		bits.Position += dbiHeader.secconSize;
		bits.Position += dbiHeader.secmapSize;
		bits.Position += dbiHeader.filinfSize;
		bits.Position += dbiHeader.tsmapSize;
		bits.Position += dbiHeader.ecinfoSize;
		num = bits.Position + dbiHeader.dbghdrSize;
		if (dbiHeader.dbghdrSize > 0)
		{
			header = new DbiDbgHdr(bits);
		}
		bits.Position = num;
	}

	internal static PdbInfo LoadFunctions(Stream read)
	{
		PdbInfo pdbInfo = new PdbInfo();
		pdbInfo.TokenToSourceMapping = new Dictionary<uint, PdbTokenLine>();
		BitAccess bitAccess = new BitAccess(65536);
		PdbFileHeader pdbFileHeader = new PdbFileHeader(read, bitAccess);
		PdbReader reader = new PdbReader(read, pdbFileHeader.pageSize);
		MsfDirectory msfDirectory = new MsfDirectory(reader, pdbFileHeader, bitAccess);
		DbiModuleInfo[] modules = null;
		Dictionary<string, PdbSource> sourceCache = new Dictionary<string, PdbSource>();
		msfDirectory.streams[1].Read(reader, bitAccess);
		Dictionary<string, int> dictionary = LoadNameIndex(bitAccess, out pdbInfo.Age, out pdbInfo.Guid);
		if (!dictionary.TryGetValue("/NAMES", out var value))
		{
			throw new PdbException("Could not find the '/NAMES' stream: the PDB file may be a public symbol file instead of a private symbol file");
		}
		msfDirectory.streams[value].Read(reader, bitAccess);
		IntHashTable names = LoadNameStream(bitAccess);
		if (!dictionary.TryGetValue("SRCSRV", out var value2))
		{
			pdbInfo.SourceServerData = string.Empty;
		}
		else
		{
			DataStream dataStream = msfDirectory.streams[value2];
			byte[] array = new byte[dataStream.contentSize];
			dataStream.Read(reader, bitAccess);
			pdbInfo.SourceServerData = bitAccess.ReadBString(array.Length);
		}
		if (dictionary.TryGetValue("SOURCELINK", out var value3))
		{
			DataStream dataStream2 = msfDirectory.streams[value3];
			pdbInfo.SourceLinkData = new byte[dataStream2.contentSize];
			dataStream2.Read(reader, bitAccess);
			bitAccess.ReadBytes(pdbInfo.SourceLinkData);
		}
		msfDirectory.streams[3].Read(reader, bitAccess);
		LoadDbiStream(bitAccess, out modules, out var header, readStrings: true);
		List<PdbFunction> list = new List<PdbFunction>();
		if (modules != null)
		{
			foreach (DbiModuleInfo dbiModuleInfo in modules)
			{
				if (dbiModuleInfo.stream > 0)
				{
					msfDirectory.streams[dbiModuleInfo.stream].Read(reader, bitAccess);
					if (dbiModuleInfo.moduleName == "TokenSourceLineInfo")
					{
						LoadTokenToSourceInfo(bitAccess, dbiModuleInfo, names, msfDirectory, dictionary, reader, pdbInfo.TokenToSourceMapping, sourceCache);
					}
					else
					{
						LoadFuncsFromDbiModule(bitAccess, dbiModuleInfo, names, list, readStrings: true, msfDirectory, dictionary, reader, sourceCache);
					}
				}
			}
		}
		PdbFunction[] array2 = list.ToArray();
		if (header.snTokenRidMap != 0 && header.snTokenRidMap != ushort.MaxValue)
		{
			msfDirectory.streams[header.snTokenRidMap].Read(reader, bitAccess);
			uint[] array3 = new uint[msfDirectory.streams[header.snTokenRidMap].Length / 4];
			bitAccess.ReadUInt32(array3);
			PdbFunction[] array4 = array2;
			foreach (PdbFunction pdbFunction in array4)
			{
				pdbFunction.token = 0x6000000u | array3[pdbFunction.token & 0xFFFFFF];
			}
		}
		Array.Sort(array2, PdbFunction.byAddressAndToken);
		pdbInfo.Functions = array2;
		return pdbInfo;
	}

	private static void LoadTokenToSourceInfo(BitAccess bits, DbiModuleInfo module, IntHashTable names, MsfDirectory dir, Dictionary<string, int> nameIndex, PdbReader reader, Dictionary<uint, PdbTokenLine> tokenToSourceMapping, Dictionary<string, PdbSource> sourceCache)
	{
		bits.Position = 0;
		bits.ReadInt32(out var value);
		if (value != 4)
		{
			throw new PdbDebugException("Invalid signature. (sig={0})", value);
		}
		bits.Position = 4;
		OemSymbol oemSymbol = default(OemSymbol);
		while (bits.Position < module.cbSyms)
		{
			bits.ReadUInt16(out var value2);
			int position = bits.Position;
			int position2 = bits.Position + value2;
			bits.Position = position;
			bits.ReadUInt16(out var value3);
			switch ((SYM)value3)
			{
			case SYM.S_OEM:
				bits.ReadGuid(out oemSymbol.idOem);
				bits.ReadUInt32(out oemSymbol.typind);
				if (oemSymbol.idOem == PdbFunction.msilMetaData)
				{
					if (bits.ReadString() == "TSLI")
					{
						bits.ReadUInt32(out var value4);
						bits.ReadUInt32(out var value5);
						bits.ReadUInt32(out var value6);
						bits.ReadUInt32(out var value7);
						bits.ReadUInt32(out var value8);
						bits.ReadUInt32(out var value9);
						if (!tokenToSourceMapping.TryGetValue(value4, out var value10))
						{
							tokenToSourceMapping.Add(value4, new PdbTokenLine(value4, value5, value6, value7, value8, value9));
						}
						else
						{
							while (value10.nextLine != null)
							{
								value10 = value10.nextLine;
							}
							value10.nextLine = new PdbTokenLine(value4, value5, value6, value7, value8, value9);
						}
					}
					bits.Position = position2;
					break;
				}
				throw new PdbDebugException("OEM section: guid={0} ti={1}", oemSymbol.idOem, oemSymbol.typind);
			case SYM.S_END:
				bits.Position = position2;
				break;
			default:
				bits.Position = position2;
				break;
			}
		}
		bits.Position = module.cbSyms + module.cbOldLines;
		int limit = module.cbSyms + module.cbOldLines + module.cbLines;
		IntHashTable intHashTable = ReadSourceFileInfo(bits, (uint)limit, names, dir, nameIndex, reader, sourceCache);
		foreach (PdbTokenLine value11 in tokenToSourceMapping.Values)
		{
			value11.sourceFile = (PdbSource)intHashTable[(int)value11.file_id];
		}
	}

	private static IntHashTable ReadSourceFileInfo(BitAccess bits, uint limit, IntHashTable names, MsfDirectory dir, Dictionary<string, int> nameIndex, PdbReader reader, Dictionary<string, PdbSource> sourceCache)
	{
		IntHashTable intHashTable = new IntHashTable();
		_ = bits.Position;
		CV_FileCheckSum cV_FileCheckSum = default(CV_FileCheckSum);
		while (bits.Position < limit)
		{
			bits.ReadInt32(out var value);
			bits.ReadInt32(out var value2);
			int position = bits.Position;
			int num = bits.Position + value2;
			if (value == 244)
			{
				while (bits.Position < num)
				{
					int key = bits.Position - position;
					bits.ReadUInt32(out cV_FileCheckSum.name);
					bits.ReadUInt8(out cV_FileCheckSum.len);
					bits.ReadUInt8(out cV_FileCheckSum.type);
					string text = (string)names[(int)cV_FileCheckSum.name];
					if (!sourceCache.TryGetValue(text, out var value3))
					{
						Guid doctype = SymDocumentType_Text;
						Guid language = Guid.Empty;
						Guid vendor = Guid.Empty;
						Guid checksumAlgo = Guid.Empty;
						byte[] checksum = null;
						if (nameIndex.TryGetValue("/SRC/FILES/" + text.ToUpperInvariant(), out var value4))
						{
							BitAccess bits2 = new BitAccess(256);
							dir.streams[value4].Read(reader, bits2);
							LoadInjectedSourceInformation(bits2, out doctype, out language, out vendor, out checksumAlgo, out checksum);
						}
						value3 = new PdbSource(text, doctype, language, vendor, checksumAlgo, checksum);
						sourceCache.Add(text, value3);
					}
					intHashTable.Add(key, value3);
					bits.Position += cV_FileCheckSum.len;
					bits.Align(4);
				}
				bits.Position = num;
			}
			else
			{
				bits.Position = num;
			}
		}
		return intHashTable;
	}
}
