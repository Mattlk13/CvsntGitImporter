/*
 * John Hall <john.hall@xjtag.com>
 * Copyright (c) Midas Yellow Ltd. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace CvsGitConverter
{
	/// <summary>
	/// Information about a file in CVS.
	/// </summary>
	class FileInfo
	{
		public readonly string Name;

		/// <summary>
		/// The tags defined on the file.
		/// </summary>
		public readonly Dictionary<string, Revision> Tags = new Dictionary<string, Revision>();

		/// <summary>
		/// The branches defined on the file.
		/// </summary>
		public readonly Dictionary<Revision, string> Branches = new Dictionary<Revision, string>();

		public FileInfo(string name)
		{
			this.Name = name;
		}

		public void AddTag(string name, Revision revision)
		{
			// work out whether it's a normal tag or a branch tag
			if (revision.IsBranch)
				Branches[revision.BranchStem] = name;
			else
				Tags[name] = revision;
		}

		/// <summary>
		/// Gets the branch for a revision.
		/// </summary>
		public string GetBranch(Revision revision)
		{
			if (revision.Parts.Count() == 2)
			{
				return "MAIN";
			}
			else
			{
				var branchStem = revision.BranchStem;
				string branchTag;
				if (!this.Branches.TryGetValue(branchStem, out branchTag))
					throw new Exception(String.Format("Branch with stem {0} not found on file {1} when looking for r{2}",
							branchStem, this.Name, revision));

				return branchTag;
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}