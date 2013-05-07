/*
 * John Hall <john.hall@camtechconsultants.com>
 * Copyright (c) Cambridge Technology Consultants Ltd. All rights reserved.
 */

using System;
using System.Text;

namespace CvsGitConverter
{
	/// <summary>
	/// The contents of a file.
	/// </summary>
	class FileContent
	{
		/// <summary>
		/// The file name.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The file's data
		/// </summary>
		public readonly FileContentData Data;

		/// <summary>
		/// Is this a file deletion?
		/// </summary>
		public bool IsDead
		{
			get { return Data.Length == 0; }
		}

		public FileContent(string path, FileContentData name)
		{
			this.Name = path;
			this.Data = name;
		}

		public static FileContent CreateDeadFile(string path)
		{
			return new FileContent(path, FileContentData.Empty);
		}
	}

	/// <summary>
	/// The raw data for a revision of a file.
	/// </summary>
	class FileContentData
	{
		/// <summary>
		/// The data. The buffer may be longer than the file, so the Length field must be used.
		/// </summary>
		public readonly byte[] Data;

		/// <summary>
		/// The length of the file's contents.
		/// </summary>
		public readonly long Length;

		public FileContentData(byte[] data, long length)
		{
			Data = data;
			Length = length;
		}

		public override string ToString()
		{
			return Encoding.Default.GetString(Data, 0, (int)Math.Max(Length, 0x100));
		}

		/// <summary>
		/// An empty file.
		/// </summary>
		public static readonly FileContentData Empty = new FileContentData(new byte[0], 0);
	}
}