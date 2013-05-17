/*
 * John Hall <john.hall@camtechconsultants.com>
 * Copyright (c) Cambridge Technology Consultants Ltd. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CvsGitConverter.Utils;

namespace CvsGitConverter
{
	/// <summary>
	/// Command line switches.
	/// </summary>
	class Switches : SwitchesDefBase
	{
		[SwitchDef(ShortSwitch="-C", LongSwitch="--config")]
		public ObservableCollection<string> Config { get; set; }

		[SwitchDef(LongSwitch="--debug", ShortSwitch="-d")]
		public bool Debug { get; set; }

		[SwitchDef(LongSwitch="--sandbox")]
		public string Sandbox { get; set; }

		[SwitchDef(LongSwitch="--cvs-cache")]
		public string CvsCache { get; set; }

		[SwitchDef(LongSwitch="--cvs-processes", Description="The number of CVS processes to run in parallel when importing. Defaults to the number of processors on the system.")]
		public string _CvsProcesses { get; set; }

		[SwitchDef(LongSwitch="--tagger-name", Description="The name to use for the user when creating tags")]
		public string TaggerName { get; set; }

		[SwitchDef(LongSwitch="--tagger-email", Description="The e-mail address to use for the user when creating tags")]
		public string TaggerEmail { get; set; }

		[SwitchDef(LongSwitch="--include-tag")]
		public ObservableCollection<string> IncludeTag { get; set; }

		[SwitchDef(LongSwitch="--exclude-tag")]
		public ObservableCollection<string> ExcludeTag { get; set; }

		[SwitchDef(LongSwitch="--include-branch")]
		public ObservableCollection<string> IncludeBranch { get; set; }

		[SwitchDef(LongSwitch="--exclude-branch")]
		public ObservableCollection<string> ExcludeBranch { get; set; }

		public readonly InclusionMatcher TagMatcher = new InclusionMatcher();

		public readonly InclusionMatcher BranchMatcher = new InclusionMatcher();

		/// <summary>
		/// Gets the number of CVS processes to run.
		/// </summary>
		public int CvsProcesses { get; private set; }

		public Switches()
		{
			Config = new ObservableCollection<string>();
			Config.CollectionChanged += Config_CollectionChanged;

			new IncludeExcludeWatcher(IncludeTag = new ObservableCollection<string>(),
					ExcludeTag = new ObservableCollection<string>(), TagMatcher);
			new IncludeExcludeWatcher(IncludeBranch = new ObservableCollection<string>(),
					ExcludeBranch = new ObservableCollection<string>(), BranchMatcher);

			CvsProcesses = Environment.ProcessorCount;
			TaggerName = Environment.GetEnvironmentVariable("USERNAME") ?? "nobody";
			TaggerEmail = String.Format("{0}@{1}", TaggerName, Environment.MachineName);
		}

		public override void Verify()
		{
			base.Verify();

			if (this.Sandbox == null)
				throw new CommandLineArgsException("No CVS repository specified");

			if (_CvsProcesses != null)
			{
				int cvsProcesses;
				if (int.TryParse(_CvsProcesses, out cvsProcesses) && cvsProcesses > 0)
					this.CvsProcesses = cvsProcesses;
				else
					throw new CommandLineArgsException("Invalid value for cvs-processes: {0}", _CvsProcesses);
			}
		}

		void ParseConfigFile(string filename)
		{
			try
			{
				var args = ReadConfigFileEntries(filename).ToArray();
				this.Parse(args);
			}
			catch (IOException ioe)
			{
				throw new CommandLineArgsException(String.Format("Unable to open {0}: {1}", filename, ioe.Message), ioe);
			}
			catch (System.Security.SecurityException se)
			{
				throw new CommandLineArgsException(String.Format("Unable to open {0}: {1}", filename, se.Message), se);
			}
			catch (UnauthorizedAccessException uae)
			{
				throw new CommandLineArgsException(String.Format("Unable to open {0}: {1}", filename, uae.Message), uae);
			}
		}

		IEnumerable<string> ReadConfigFileEntries(string filename)
		{
			int lineNo = 0;
			foreach (var rawLine in File.ReadLines(filename, Encoding.UTF8))
			{
				lineNo++;
				var line = Regex.Replace(rawLine, @"#.*$", "");
				if (Regex.IsMatch(line, @"^\s*$"))
					continue;

				var match = Regex.Match(line, @"^(\S+)\s+(.*)");
				if (!match.Success)
					throw new CommandLineArgsException("{0}({1}): unrecognised input", filename, lineNo);

				yield return String.Format("--{0}", match.Groups[1].Value);
				yield return match.Groups[2].Value.Trim();
			}
		}


		void Config_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Parse config files as they're encountered
			if (e.Action == NotifyCollectionChangedAction.Add)
				ParseConfigFile(e.NewItems[0] as string);
		}


		/// <summary>
		/// Watch two collections of include and exclude rules and update the rules in
		/// an InclusionMatcher instance. Means that the rules get added in the correct order.
		/// </summary>
		private class IncludeExcludeWatcher
		{
			private readonly InclusionMatcher m_matcher;

			public IncludeExcludeWatcher(ObservableCollection<string> includes, ObservableCollection<string> excludes,
					InclusionMatcher matcher)
			{
				m_matcher = matcher;
				includes.CollectionChanged += Include_CollectionChanged;
				excludes.CollectionChanged += Exclude_CollectionChanged;
			}

			void AddRule(string pattern, bool include)
			{
				Regex regex;
				try
				{
					regex = new Regex(pattern);
				}
				catch (ArgumentException)
				{
					throw new CommandLineArgsException("Invalid regex: {0}", pattern);
				}

				if (include)
					m_matcher.AddIncludeRule(regex);
				else
					m_matcher.AddExcludeRule(regex);
			}

			void Include_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				AddRule(e.NewItems[0] as string, true);
			}

			void Exclude_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				AddRule(e.NewItems[0] as string, false);
			}
		}
	}
}