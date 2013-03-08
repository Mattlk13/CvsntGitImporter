/*
 * John Hall <john.hall@xjtag.com>
 * Copyright (c) Midas Yellow Ltd. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CvsGitConverter
{
	/// <summary>
	/// Tracks the versions of all files in the repository for a specific branch.
	/// </summary>
	class RepositoryBranchState
	{
		private readonly Dictionary<string, Revision> m_files = new Dictionary<string, Revision>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets or sets the current revision of a file.
		/// </summary>
		public Revision this[string filename]
		{
			get
			{
				Revision value;
				if (m_files.TryGetValue(filename, out value))
					return value;
				else
					return Revision.Empty;
			}
			set
			{
				var previousRevision = this[filename];

				if (!previousRevision.DirectlyPrecedes(value))
				{
					throw new RepositoryConsistencyException(String.Format(
							"Revision r{0} in {1} did not directly precede r{2}",
							value, previousRevision, filename));
				}

				m_files[filename] = value;
			}
		}

		/// <summary>
		/// Gets all currently live files.
		/// </summary>
		public IEnumerable<string> LiveFiles
		{
			get { return m_files.Keys; }
		}

		public RepositoryBranchState(string branch)
		{
		}

		/// <summary>
		/// Apply a commit.
		/// </summary>
		public void Apply(Commit commit)
		{
			foreach (var f in commit)
			{
				if (f.IsDead)
					m_files.Remove(f.File.Name);
				else
					m_files[f.File.Name] = f.Revision;
			}
		}
	}
}