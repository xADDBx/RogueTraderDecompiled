using System;
using System.IO;
using System.Security.Cryptography;
using Mono.Cecil.PE;
using Mono.Security.Cryptography;

namespace Mono.Cecil;

internal static class CryptoService
{
	public static byte[] GetPublicKey(WriterParameters parameters)
	{
		using RSA rsa = parameters.CreateRSA();
		byte[] array = Mono.Security.Cryptography.CryptoConvert.ToCapiPublicKeyBlob(rsa);
		byte[] array2 = new byte[12 + array.Length];
		Buffer.BlockCopy(array, 0, array2, 12, array.Length);
		array2[1] = 36;
		array2[4] = 4;
		array2[5] = 128;
		array2[8] = (byte)array.Length;
		array2[9] = (byte)(array.Length >> 8);
		array2[10] = (byte)(array.Length >> 16);
		array2[11] = (byte)(array.Length >> 24);
		return array2;
	}

	public static void StrongName(Stream stream, ImageWriter writer, WriterParameters parameters)
	{
		int strong_name_pointer;
		byte[] strong_name = CreateStrongName(parameters, HashStream(stream, writer, out strong_name_pointer));
		PatchStrongName(stream, strong_name_pointer, strong_name);
	}

	private static void PatchStrongName(Stream stream, int strong_name_pointer, byte[] strong_name)
	{
		stream.Seek(strong_name_pointer, SeekOrigin.Begin);
		stream.Write(strong_name, 0, strong_name.Length);
	}

	private static byte[] CreateStrongName(WriterParameters parameters, byte[] hash)
	{
		using RSA key = parameters.CreateRSA();
		RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(key);
		rSAPKCS1SignatureFormatter.SetHashAlgorithm("SHA1");
		byte[] array = rSAPKCS1SignatureFormatter.CreateSignature(hash);
		Array.Reverse((Array)array);
		return array;
	}

	private static byte[] HashStream(Stream stream, ImageWriter writer, out int strong_name_pointer)
	{
		Section text = writer.text;
		int headerSize = (int)writer.GetHeaderSize();
		int pointerToRawData = (int)text.PointerToRawData;
		DataDirectory strongNameSignatureDirectory = writer.GetStrongNameSignatureDirectory();
		if (strongNameSignatureDirectory.Size == 0)
		{
			throw new InvalidOperationException();
		}
		strong_name_pointer = (int)(pointerToRawData + (strongNameSignatureDirectory.VirtualAddress - text.VirtualAddress));
		int size = (int)strongNameSignatureDirectory.Size;
		SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] buffer = new byte[8192];
		using (CryptoStream dest_stream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
		{
			stream.Seek(0L, SeekOrigin.Begin);
			CopyStreamChunk(stream, dest_stream, buffer, headerSize);
			stream.Seek(pointerToRawData, SeekOrigin.Begin);
			CopyStreamChunk(stream, dest_stream, buffer, strong_name_pointer - pointerToRawData);
			stream.Seek(size, SeekOrigin.Current);
			CopyStreamChunk(stream, dest_stream, buffer, (int)(stream.Length - (strong_name_pointer + size)));
		}
		return sHA1Managed.Hash;
	}

	public static void CopyStreamChunk(Stream stream, Stream dest_stream, byte[] buffer, int length)
	{
		while (length > 0)
		{
			int num = stream.Read(buffer, 0, System.Math.Min(buffer.Length, length));
			dest_stream.Write(buffer, 0, num);
			length -= num;
		}
	}

	public static byte[] ComputeHash(string file)
	{
		if (!File.Exists(file))
		{
			return Empty<byte>.Array;
		}
		using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
		return ComputeHash(stream);
	}

	public static byte[] ComputeHash(Stream stream)
	{
		SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] buffer = new byte[8192];
		using (CryptoStream dest_stream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
		{
			CopyStreamChunk(stream, dest_stream, buffer, (int)stream.Length);
		}
		return sHA1Managed.Hash;
	}

	public static byte[] ComputeHash(params ByteBuffer[] buffers)
	{
		SHA1Managed sHA1Managed = new SHA1Managed();
		using (CryptoStream cryptoStream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
		{
			for (int i = 0; i < buffers.Length; i++)
			{
				cryptoStream.Write(buffers[i].buffer, 0, buffers[i].length);
			}
		}
		return sHA1Managed.Hash;
	}

	public static Guid ComputeGuid(byte[] hash)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy(hash, 0, array, 0, 16);
		array[7] = (byte)((array[7] & 0xFu) | 0x40u);
		array[8] = (byte)((array[8] & 0x3Fu) | 0x80u);
		return new Guid(array);
	}
}
