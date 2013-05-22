/*
 * John Hall <john.hall@camtechconsultants.com>
 * Copyright (c) Cambridge Technology Consultants Ltd. All rights reserved.
 */

using System.Collections.Generic;

namespace CTC.CvsntGitImporter
{
	/// <summary>
	/// Tracks the state of the repository allowing commits to be replayed.
	/// </summary>
	class RepositoryState
	{
		private readonly Dictionary<string, RepositoryBranchState> m_branches = new Dictionary<string, RepositoryBranchState>();

		/// <summary>
		/// Gets the state for a branch.
		/// </summary>
		public RepositoryBranchState this[string branch]
		{
			get
			{
				RepositoryBranchState state;
				if (m_branches.TryGetValue(branch, out state))
					return state;

				state = new RepositoryBranchState(branch);
				m_branches[branch] = state;
				return state;
			}
		}

		/// <summary>
		/// Apply a commit.
		/// </summary>
		public void Apply(Commit commit)
		{
			this[commit.Branch].Apply(commit);
		}
	}
}