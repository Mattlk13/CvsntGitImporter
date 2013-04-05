﻿/*
 * John Hall <john.hall@camtechconsultants.com>
 * Copyright (c) Cambridge Technology Consultants Ltd. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CvsGitConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CvsGitTest
{
	/// <summary>
	/// Unit tests for the TagResolver class.
	/// </summary>
	[TestClass]
	public class TagResolverTest
	{
		private ILogger m_logger;

		[TestInitialize]
		public void Setup()
		{
			m_logger = MockRepository.GenerateStub<ILogger>();
		}

		[TestMethod]
		public void Resolve_ReorderCommits()
		{
			var commits = CreateCommitThatNeedsReordering().ToList();
			var allFiles = CreateAllFiles(commits);

			var resolver = new TagResolver(m_logger, commits, allFiles, new InclusionMatcher());
			var result = resolver.Resolve();

			Assert.IsFalse(result, "Failed");
		}

		[TestMethod]
		public void ResolveAndFix_ReorderCommits()
		{
			var commits = CreateCommitThatNeedsReordering();
			var orderBefore = commits.ToList();
			var allFiles = CreateAllFiles(commits);

			var resolver = new TagResolver(m_logger, commits, allFiles, new InclusionMatcher());
			var result = resolver.ResolveAndFix();

			Assert.IsTrue(result, "Succeeded");
			Assert.IsTrue(resolver.Commits.SequenceEqual(new[] { orderBefore[0], orderBefore[2], orderBefore[1] }), "Commits reordered");
		}

		[TestMethod]
		public void Resolve_SplitCommit()
		{
			var commits = CreateCommitThatNeedsSplitting().ToList();
			var allFiles = CreateAllFiles(commits);

			var resolver = new TagResolver(m_logger, commits, allFiles, new InclusionMatcher());
			var result = resolver.Resolve();

			Assert.IsFalse(result, "Failed");
		}

		[TestMethod]
		public void ResolveAndFix_SplitCommit()
		{
			var commits = CreateCommitThatNeedsSplitting().ToList();
			var orderBefore = commits.ToList();
			var allFiles = CreateAllFiles(commits);

			var resolver = new TagResolver(m_logger, commits, allFiles, new InclusionMatcher());
			var result = resolver.ResolveAndFix();

			Assert.IsTrue(result, "Succeeded");
			var newCommits = resolver.Commits.ToList();
			Assert.AreEqual(newCommits.Count, 4);
			Assert.AreEqual(newCommits[0], orderBefore[0]);
			Assert.AreEqual(newCommits[1], orderBefore[1]);
			Assert.IsTrue(newCommits[2].Single().File.Name == "file2" && newCommits[2].Single().Revision.Equals(Revision.Create("1.2")));
			Assert.IsTrue(newCommits[3].Single().File.Name == "file1" && newCommits[3].Single().Revision.Equals(Revision.Create("1.3")));
		}


		private static IEnumerable<Commit> CreateCommitThatNeedsReordering()
		{
			var file1 = new FileInfo("file1");
			file1.AddTag("tag", Revision.Create("1.1"));

			var file2 = new FileInfo("file2");
			file2.AddTag("tag", Revision.Create("1.2"));

			var id0 = "id0";
			var commit0 = new Commit(id0)
			{
				CreateFileRevision(file1, "1.1", id0),
				CreateFileRevision(file2, "1.1", id0),
			};
			var id1 = "id1";
			var commit1 = new Commit(id1)
			{
				CreateFileRevision(file1, "1.2", id1),
			};
			var id2 = "id2";
			var commit2 = new Commit(id2)
			{
				CreateFileRevision(file2, "1.2", id2),
			};

			return new[] { commit0, commit1, commit2 };
		}

		private static IEnumerable<Commit> CreateCommitThatNeedsSplitting()
		{
			var file1 = new FileInfo("file1");
			file1.AddTag("tag", Revision.Create("1.2"));

			var file2 = new FileInfo("file2");
			file2.AddTag("tag", Revision.Create("1.2"));

			var id0 = "id0";
			var commit0 = new Commit(id0)
			{
				CreateFileRevision(file1, "1.1", id0),
				CreateFileRevision(file2, "1.1", id0),
			};
			var id1 = "id1";
			var commit1 = new Commit(id1)
			{
				CreateFileRevision(file1, "1.2", id1),
			};
			var id2 = "id2";
			var commit2 = new Commit(id2)
			{
				CreateFileRevision(file1, "1.3", id2),
				CreateFileRevision(file2, "1.2", id2),
			};

			return new[] { commit0, commit1, commit2 };
		}

		private static Dictionary<string, FileInfo> CreateAllFiles(IEnumerable<Commit> commits)
		{
			var allFiles = new Dictionary<string, FileInfo>();

			foreach (var f in commits.SelectMany(c => c.Select(r => r.File)).Distinct())
				allFiles.Add(f.Name, f);

			return allFiles;
		}

		private static FileRevision CreateFileRevision(FileInfo file, string revision, string commitId)
		{
			return new FileRevision(file, Revision.Create(revision), Revision.Empty, DateTime.Now,
					"fred", commitId);
		}
	}
}
