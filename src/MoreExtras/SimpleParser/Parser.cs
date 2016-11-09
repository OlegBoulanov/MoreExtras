using System.Collections.Generic;

namespace MoreExtras.SimpleParser
{
	public class Parser
	{
		// IParser implementation
		public Dictionary<string, AttrTree> Grammars { get; protected set; } = new Dictionary<string, AttrTree>();
		public Dictionary<string, string> Lexicon { get; protected set; } = new Dictionary<string, string>();
		public void Prepare(AttrTree tree)
		{
			Grammars.Clear();
			foreach (AttrTree g in tree) Grammars.Add(g.Node, g);
			foreach (AttrTree g in tree) ScanLexicon(g);
		}
		protected void ScanLexicon(AttrTree tree)
		{
			foreach (var t in tree)
			{
				if (t.Count > 0) ScanLexicon(t);
				else if (!Grammars.ContainsKey(t.Node) && !Lexicon.ContainsKey(t.Node)) Lexicon.Add(t.Node, null);
			}
		}
		public SemTree Parse(AttrTree g, string[] words)
		{
			if (g == null) return null;
			if (words.Length == 0)
			{
				// empty word list should return empty tree
				SemTree st = new SemTree();
				st.Node = g.Node;
				return st;
			}
			// enum clauses
			foreach (AttrTree c in g)
			{
				SemTree res = new SemTree();
				res.Node = g.Node;
				int i_patt = 0;
				if (_Match(c, 0, words, ref i_patt, ref res))
				{
					res.Attributes = c.Attributes;
					return res;
				}
			}
			// no match = word list out of grammar
			return null;
		}

		protected bool _Match(AttrTree targ, int i_targ, string[] patt, ref int i_patt, ref SemTree result)
		{
			bool tend = i_targ >= targ.Count;
			bool pend = i_patt >= patt.Length;
			if (tend) return pend;
			int tit = i_targ;
			if (pend)
			{
				// the rest must be optional
				while (tit < targ.Count)
				{
					if (!((AttrTree)targ[tit++]).IsOptional) return false;
				}
				return true;
			}
			AttrTree goal = (AttrTree)targ[tit++];
			if (goal.Node == Constants.Grammar.UNIVERSAL)
			{
				// consume all words
				while (i_patt < patt.Length) result.Add(new SemTree(patt[i_patt++], goal.Attributes));
				return true;
			}
			// word
			string word = patt[i_patt];
			if (goal.Node == word)
			{
				int pit = i_patt + 1;
				if (_Match(targ, tit, patt, ref pit, ref result))
				{
					SemTree at = new SemTree(word, goal.Attributes);
					result.Insert(0, at);
					i_patt = pit;
					return true;
				}
			}
			// prefer to skip optionals
			if (goal.IsOptional && _Match(targ, tit, patt, ref i_patt, ref result)) return true;
			// find class
			if (!Grammars.ContainsKey(goal.Node)) return false;
			AttrTree cls = Grammars[goal.Node];
			// parse it
			foreach (AttrTree clause in cls)
			{
				// concatenate clause and the rest of target (after goal) to the new tree
				AttrTree temp_targ = new AttrTree(clause);
				for (int j = tit; j < targ.Count; j++) temp_targ.Add(targ[j]);
				// try to match
				int temp_pit = i_patt;
				if (_Match(temp_targ, 0, patt, ref temp_pit, ref result))
				{
					// create subtree
					SemTree subtree = new SemTree(cls.Node, clause.Attributes);
					// move nodes to subtree
					foreach (var c in clause)
					{
						if (result.Count == 0) break;
						AttrTree p = result[0];
						if (p.Node != c.Node) continue;	// ignore skipped optionals
						subtree.Add(p);
						result.RemoveAt(0);
					}
					result.Insert(0, subtree);
					i_patt = temp_pit;
					return true;
				}
			}
			return false;
		}

		public bool PartialParse(AttrTree g, string[] words, ref List<string> result)
		{
			if (g == null) return false;
			bool res = false;
			foreach (AttrTree c in g)
			{
				int i_patt = 0;
				if (_PartialMatch(c, 0, words, i_patt, result)) res = true;
			}
			return res;
		}

		protected bool _Next(AttrTree r, int i, List<string> result)
		{
			bool res = false;
			for (; i < r.Count; i++)
			{	// loop until not optional
				AttrTree g = (AttrTree)r[i];
				if (Grammars.ContainsKey(g.Node))
				{
					foreach (AttrTree alt in (AttrTree)Grammars[g.Node])
					{
						if (_Next(alt, 0, result)) res = true;
					}
				}
				else
				{
					if (!result.Contains(g.Node)) result.Add(g.Node);
					res = true;
				}
				if (!g.IsOptional) break;
			}
			return res;
		}

		protected void Merge(List<string> a, List<string> list)
		{
			foreach (var o in a)
			{
				if (!list.Contains(o)) list.Add(o);
			}
		}

		protected bool _PartialMatch(AttrTree targ, int i_targ, string[] patt, int i_patt, List<string> result)
		{
			bool pend = patt.Length <= i_patt;
			if (targ.Count <= i_targ) return pend;
			if (pend)
			{
				return _Next(targ, i_targ, result);
			}
			AttrTree goal = (AttrTree)targ[i_targ++];
			string word = patt[i_patt];
			// word
			if (goal.Node.Equals(word))
			{
				return _PartialMatch(targ, i_targ, patt, i_patt + 1, result);
			}
			bool res = goal.IsOptional && _PartialMatch(targ, i_targ, patt, i_patt, result);
			// grammar
			if (!Grammars.ContainsKey(goal.Node)) return false;
			AttrTree grm = (AttrTree)Grammars[goal.Node];
			// parse all the alternatives
			foreach (AttrTree alt in grm)
			{
				// concatenate clause and the rest of target (after goal) to the new tree
				AttrTree temp_targ = new AttrTree(alt);
				for (int j = i_targ; j < targ.Count; j++) temp_targ.Add(targ[j]);
				// try to match
				if (_PartialMatch(temp_targ, 0, patt, i_patt, result)) res = true;
			}
			return res;
		}

	}
}
