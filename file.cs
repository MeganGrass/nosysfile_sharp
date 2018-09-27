using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.InteropServices;

/*
 * Generic File Handler
 * Megan Grass
 */
namespace System_File
{

	/*
	 * FILE
	 * 
	 * Explanation:
	 *		Simple binary file handler.
	 */
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	internal class FILE
	{
		// Required for Write()
		private bool ReadOnly = false;

		/*
		 * ErrorExists
		 * 
		 * Syntax:
		 *		bool ErrorExists(
		 *			FileStream _File	// File to test
		 *		)
		 * 
		 * Explanation:
		 *		Test an initialized file for common access errors.
		 * 
		 * Return Value:
		 *		true, if an error would occur when operations continue; false otherwise
		 */
		private bool ErrorExists(FileStream _File)
		{
			if (_File == null)
			{
				Console.Write("Attempting I/O operations with an uninitialized file stream, aborting...\r\n");
				return true;
			}
			FileAttributes Attributes = File.GetAttributes(_File.Name);
			if ((Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) { ReadOnly = true; }
			else { ReadOnly = false; }
			if ((Attributes & FileAttributes.Compressed) == FileAttributes.Compressed)
			{
				Console.Write("{0} is compressed, aborting...\r\n", _File.Name.ToString());
				return true;
			}
			if ((Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				Console.Write("{0} is a directory, aborting...\r\n", _File.Name.ToString());
				return true;
			}
			if ((Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
			{
				Console.Write("{0} is encrypted, aborting...\r\n", _File.Name.ToString());
				return true;
			}
			if ((Attributes & FileAttributes.Offline) == FileAttributes.Offline)
			{
				Console.Write("{0} is offline, aborting...\r\n", _File.Name.ToString());
				return true;
			}
			return false;
		}

		/*
		 * Open
		 * 
		 * Syntax:
		 *		FileStream Open(
		 *			string Name			// Name of file
		 *		)
		 * 
		 * Explanation:
		 *		Open a file from disk for reading and writing.
		 * 
		 * Return Value:
		 *		FileStream of Name if successful; null otherwise
		 */
		public FileStream Open(string Name)
		{
			if (!File.Exists(Name)) { Console.Write("{0} doesn't exist!\r\n", Name.ToString()); return null; }
			else
			{
				FileStream _File = null;
				try
				{
					_File = File.Open(Name, FileMode.Open);
					if (ErrorExists(_File)) { return null; }
				}
				catch (Exception Error)
				{
					Console.Write("{0}\r\n", Error.Message);
					return null;
				}
				return _File;
			}
		}

		/*
		 * Length
		 * 
		 * Syntax:
		 *		long Length(
		 *			FileStream _File	// Any open FileStream
		 *		)
		 *		long Length(
		 *			string Name			// Name of file
		 *		)
		 * 
		 * Explanation:
		 *		Return the size of a file.
		 * 
		 * Return Value:
		 *		The size of a file in bytes; zero (0) otherwise
		 */
		public long Length(FileStream _File) { return _File.Length; }
		public long Length(string Name)
		{
			using (FileStream _File = Open(Name)) { return Length(_File); }
		}

		/*
		 * Create
		 * 
		 * Syntax:
		 *		FileStream Create(
		 *			byte[] _Buffer		// Memory buffer, where data is stored
		 *			bool Clear			// Clear the buffer?
		 *			string Name			// Name of file to be created
		 *		)
		 * 
		 * Explanation:
		 *		Create a FileStream from a byte[] array.
		 * 
		 * Return Value:
		 *		FileStream; null otherwise
		 */
		public FileStream Create(byte[] _Buffer, bool Clear, string Name)
		{
			if (_Buffer.Length == 0) { Console.Write("Attempting to read from an uninitialized buffer, aborting...\r\n"); return null; }
			FileStream _File = null;
			try
			{
				_File = File.Open(Name, FileMode.Create, FileAccess.ReadWrite);
				using (MemoryStream Stream = new MemoryStream())
				{
					Stream.Write(_Buffer, 0, _Buffer.Length);
					Stream.WriteTo(_File);
				}
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return null;
			}
			finally { if (Clear) { Array.Clear(_Buffer, 0, _Buffer.Length); } }
			return _File;
		}

		/*
		 * Dummy
		 * 
		 * Syntax:
		 *		void Dummy(
		 *			long Size			// Size of file to create
		 *			string Name			// Name of file to create
		 *		)
		 * 
		 * Explanation:
		 *		Create a dummy (pad) file; all data is zero (0).
		 * 
		 * Return Value:
		 *		N/A
		 */
		public void Dummy(long Size, string Name)
		{
			byte[] _Buffer = new byte[Size];
			using (FileStream _File = Create(_Buffer, true, Name)) { }
		}

		/*
		 * Read
		 * 
		 * Syntax:
		 *		byte[] Read(
		 *			FileStream _File	// Any open FileStream
		 *			long _Offset		// Absolute pointer to where data will be read
		 *			byte[] _Buffer		// Memory buffer, where data is stored
		 *			long _ElementSize	// Amount of bytes to be read (Size of _Buffer)
		 *		)
		 * 
		 * Explanation:
		 *		Read data from an open FileStream and store it in a memory buffer.
		 * 
		 * Return Value:
		 *		byte[_ElementSize] array of data at _Offset in _File; null otherwise
		 */
		public byte[] Read(FileStream _File, long _Offset, byte[] _Buffer, long _ElementSize)
		{
			// Error
			if (ErrorExists(_File)) { return null; }
			if (_File.CanRead == false) { Console.Write("Unable to read from file, aborting...\r\n"); return null; }
			if (_File.CanSeek == false) { Console.Write("Unable to seek file pointer, aborting...\r\n"); return null; }
			if (_Buffer.Length == 0) { Console.Write("Attempting to write to an uninitialized buffer, aborting...\r\n"); return null; }

			// Read
			try
			{
				_File.Seek(_Offset, SeekOrigin.Begin);
				using (BinaryReader Binary = new BinaryReader(_File)) { Binary.Read(_Buffer, 0x00, (int)_ElementSize); }
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return null;
			}

			// Terminate
			return _Buffer;
		}

		/*
		 * Write
		 * 
		 * Syntax:
		 *		byte[] Write(
		 *			FileStream _File	// Any open FileStream
		 *			long _Offset		// Absolute pointer to where data will be written
		 *			byte[] _Buffer		// Memory buffer, where data is stored
		 *			long _ElementSize	// Amount of bytes to be written (Size of _Buffer)
		 *		)
		 * 
		 * Explanation:
		 *		Write data from a memory buffer to an open FileStream.
		 * 
		 * Return Value:
		 *		Size of bytes written to file; 0 otherwise
		 */
		public long Write(FileStream _File, long _Offset, byte[] _Buffer, long _ElementSize)
		{
			// Error
			if (ErrorExists(_File)) { return 0; }
			if (_File.CanWrite == false) { Console.Write("Unable to write to file, aborting...\r\n"); return 0; }
			if (_File.CanSeek == false) { Console.Write("Unable to seek file pointer, aborting...\r\n"); return 0; }
			if (_Buffer.Length == 0) { Console.Write("Attempting to read from an uninitialized buffer, aborting...\r\n"); return 0; }
			if (ReadOnly) { Console.Write("Attempting to write to a read-only file, aborting...\r\n"); return 0; }

			// Write
			try
			{
				_File.Seek(_Offset, SeekOrigin.Begin);
				using (BinaryWriter Binary = new BinaryWriter(_File)) { Binary.Write(_Buffer, 0x00, (int)_ElementSize); }
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return 0;
			}

			// Terminate
			return _ElementSize;
		}

		/*
		 * Buffer
		 * 
		 * Syntax:
		 *		byte[] Buffer(
		 *			FileStream _File	// Any open FileStream
		 *		)
		 *		byte[] Buffer(
		 *			string Name			// Name of file
		 *		)
		 * 
		 * Explanation:
		 *		Return a byte array of a FileStream.
		 *		
		 * Return Value:
		 *		byte array of entire file; null otherwise
		 */
		public byte[] GetBytes(FileStream _File)
		{
			if (ErrorExists(_File)) { return null; }
			try
			{
				using (MemoryStream Stream = new MemoryStream())
				{
					_File.CopyTo(Stream);
					return Stream.ToArray();
				}
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return null;
			}
		}
		public byte[] GetBytes(string Name) { return File.ReadAllBytes(Name); }

		/*
		 * GetString
		 * 
		 * Syntax:
		 *		byte[] GetString(
		 *			string Name			// Name of file
		 *		)
		 * 
		 * Explanation:
		 *		Load a text file.
		 * 
		 * Return Value:
		 *		string of entire file; null otherwise
		 */
		public string GetString(Encoding _Encoding, string Name)
		{
			byte[] _Buffer = GetBytes(Name);
			if (_Buffer.Length == 0) { Console.Write("Attempting to read from an uninitialized buffer, aborting...\r\n"); return null; }
			try
			{
				if (_Encoding == Encoding.ASCII)
				{
					ASCIIEncoding enc = new ASCIIEncoding();
					return enc.GetString(_Buffer);
				}
				else if (_Encoding == Encoding.Unicode)
				{
					UnicodeEncoding enc = new UnicodeEncoding();
					return enc.GetString(_Buffer);
				}
				else if (_Encoding == Encoding.UTF32)
				{
					UTF32Encoding enc = new UTF32Encoding();
					return enc.GetString(_Buffer);
				}
				else if (_Encoding == Encoding.UTF7)
				{
					UTF7Encoding enc = new UTF7Encoding();
					return enc.GetString(_Buffer);
				}
				else if (_Encoding == Encoding.UTF8)
				{
					UTF8Encoding enc = new UTF8Encoding();
					return enc.GetString(_Buffer);
				}
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return null;
			}
			return null;
		}

		/*
		 * Print
		 * 
		 * Syntax:
		 *		byte[] Print(
		 *			FileStream _File	// Any open FileStream
		 *			long _Offset		// Absolute pointer to where string will be written
		 *			Encoding _Encoding	// Encoding of printed string
		 *			string _String		// String buffer
		 *		)
		 *		byte[] Print(
		 *			FileStream _File	// Any open FileStream
		 *			Encoding _Encoding	// Encoding of printed string
		 *			string _String		// String buffer
		 *		)
		 *		byte[] Print(
		 *			FileStream _File	// Any open FileStream
		 *			long _Offset		// Absolute pointer to where string will be written
		 *			string _String		// String buffer
		 *		)
		 *		byte[] Print(
		 *			FileStream _File	// Any open FileStream
		 *			string _String		// String buffer
		 *		)
		 * 
		 * Explanation:
		 *		Write a formatted string to a binary file. Null-termination isn't provided.
		 *		
		 *		This function overwrites _String.Length bytes at _Offset. If the eof (end of file) is
		 *		reached, the file is expanded accordingly and the string is completely written.
		 *		
		 *		_String is always encoded using the _Encoding parameter, regardless of its original
		 *		formatting and encoding.
		 *		
		 *		The overloaded Print() functions without the _Offset parameter will simply print at
		 *		the eof (end of file).
		 *		
		 *		The two overloaded Print() functions without _Encoding always force ASCII printing.
		 * 
		 * Return Value:
		 *		true on success; false otherwise
		 */
		public bool Print(FileStream _File, long _Offset, Encoding _Encoding, string _String)
		{
			if (ErrorExists(_File)) { return false; }
			try
			{
				if (_Encoding == Encoding.ASCII)
				{
					ASCIIEncoding enc = new ASCIIEncoding();
					byte[] _Buffer = enc.GetBytes(_String);
					Write(_File, _Offset, _Buffer, _Buffer.Length);
				}
				else if (_Encoding == Encoding.Unicode)
				{
					UnicodeEncoding enc = new UnicodeEncoding();
					byte[] _Buffer = enc.GetBytes(_String);
					Write(_File, _Offset, _Buffer, _Buffer.Length);
				}
				else if (_Encoding == Encoding.UTF32)
				{
					UTF32Encoding enc = new UTF32Encoding();
					byte[] _Buffer = enc.GetBytes(_String);
					Write(_File, _Offset, _Buffer, _Buffer.Length);
				}
				else if (_Encoding == Encoding.UTF7)
				{
					UTF7Encoding enc = new UTF7Encoding();
					byte[] _Buffer = enc.GetBytes(_String);
					Write(_File, _Offset, _Buffer, _Buffer.Length);
				}
				else if (_Encoding == Encoding.UTF8)
				{
					UTF8Encoding enc = new UTF8Encoding();
					byte[] _Buffer = enc.GetBytes(_String);
					Write(_File, _Offset, _Buffer, _Buffer.Length);
				}
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return false;
			}
			return true;
		}
		public bool Print(FileStream _File, Encoding _Encoding, string _String)
		{
			return Print(_File, _File.Length, _Encoding, _String);
		}
		public bool Print(FileStream _File, long _Offset, string _String)
		{
			return Print(_File, _Offset, Encoding.ASCII, _String);
		}
		public bool Print(FileStream _File, string _String)
		{
			return Print(_File, _File.Length, Encoding.ASCII, _String);
		}

		/*
		 * Align
		 * 
		 * Syntax:
		 *		byte[] Align(
		 *			long Sector			// Disk sector size
		 *			string FileName		// Name of file to be aligned
		 *		)
		 * 
		 * Explanation:
		 *		Partition alignment; Align a file's data size to match a given Sector
		 *		byte size.
		 *		
		 *		This function is useful only for disk types (SD, CD-ROM, etc) that
		 *		require all file data to be aligned by sector size.
		 * 
		 * Return Value:
		 *		true on success; false otherwise
		 */
		public bool Align(long Sector, string FileName)
		{
			try
			{
				using (FileStream _File = Open(FileName))
				{
					if (_File == null) { return false; }
					long newLen = (_File.Length + Sector - 1) / Sector * Sector;
					byte[] _Buffer = new byte[sizeof(byte)];
					Write(_File, (newLen - sizeof(byte)), _Buffer, sizeof(byte));
					_File.Close();
				}
			}
			catch (Exception Error)
			{
				Console.Write("{0}\r\n", Error.Message);
				return false;
			}

			return true;
		}

	}

}
