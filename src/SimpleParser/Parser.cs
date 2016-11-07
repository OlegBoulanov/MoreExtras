using System;
using System.IO;
using System.Text;
using System.Collections;

namespace Eliza.Net
{
	public class Parser : IParser
	{
		// implementation specific
		protected IDictionary _grammars;
		protected IDictionary _lexicon;
		// constructor
		public Parser()
		{
			_grammars = new Hashtable();
			_lexicon = new Hashtable();
		}
		// IParser implementation
		public IDictionary Grammars { get { return _grammars; } }
		public IDictionary Lexicon { get { return _lexicon; } }
		public void Prepare(IAttrTree tree)
		{
			_grammars.Clear();
			foreach (AttrTree g in tree) _grammars.Add(g.Node, g);
			foreach (AttrTree g in tree) ScanLexicon(g);
		}
		protected void ScanLexicon(IAttrTree tree)
		{
			foreach (IAttrTree t in tree)
			{
				if (t.Count > 0) ScanLexicon(t);
				else if (!_grammars.Contains(t.Node) && !_lexicon.Contains(t.Node)) _lexicon.Add(t.Node, null);
			}
		}
		public ISemTree Parse(IAttrTree g, string[] words)
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
			foreach (IAttrTree c in g)
			{
				ISemTree res = new SemTree();
				res.Node = g.Node;
				int i_patt = 0;
				if (_Match(c, 0, words, ref i_patt, ref res))
				{
					res.Attr = c.Attr;
					return res;
				}
			}
			// no match = word list out of grammar
			return null;
		}

		protected bool _Match(IAttrTree targ, int i_targ, string[] patt, ref int i_patt, ref ISemTree result)
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
					if (!((IAttrTree)targ[tit++]).Optional) return false;
				}
				return true;
			}
			IAttrTree goal = (IAttrTree)targ[tit++];
			if (goal.Node == Consts.Grammar.UNIVERSAL)
			{
				// consume all words
				while (i_patt < patt.Length) result.Add(new SemTree(patt[i_patt++], goal.Attr));
				return true;
			}
			// word
			string word = patt[i_patt];
			if (goal.Node == word)
			{
				int pit = i_patt + 1;
				if (_Match(targ, tit, patt, ref pit, ref result))
				{
					ISemTree at = new SemTree(word, goal.Attr);
					result.Insert(0, at);
					i_patt = pit;
					return true;
				}
			}
			// prefer to skip optionals
			if (goal.Optional && _Match(targ, tit, patt, ref i_patt, ref result)) return true;
			// find class
			if (!_grammars.Contains(goal.Node)) return false;
			IAttrTree cls = (IAttrTree)_grammars[goal.Node];
			// parse it
			foreach (IAttrTree clause in cls)
			{
				// concatenate clause and the rest of target (after goal) to the new tree
				IAttrTree temp_targ = new AttrTree(clause);
				for (int j = tit; j < targ.Count; j++) temp_targ.Add(targ[j]);
				// try to match
				int temp_pit = i_patt;
				if (_Match(temp_targ, 0, patt, ref temp_pit, ref result))
				{
					// create subtree
					ISemTree subtree = new SemTree(cls.Node, clause.Attr);
					// move nodes to subtree
					foreach (AttrTree c in clause)
					{
						if (result.Count == 0) break;
						IAttrTree p = (IAttrTree)result[0];
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

		public bool PartialParse(IAttrTree g, string[] words, ref ArrayList result)
		{
			if (g == null) return false;
			bool res = false;
			foreach (IAttrTree c in g)
			{
				int i_patt = 0;
				if (_PartialMatch(c, 0, words, i_patt, result)) res = true;
			}
			return res;
		}

		protected bool _Next(IAttrTree r, int i, ArrayList result)
		{
			bool res = false;
			for (; i < r.Count; i++)
			{	// loop until not optional
				IAttrTree g = (IAttrTree)r[i];
				if (_grammars.Contains(g.Node))
				{
					foreach (IAttrTree alt in (IAttrTree)_grammars[g.Node])
					{
						if (_Next(alt, 0, result)) res = true;
					}
				}
				else
				{
					if (!result.Contains(g.Node)) result.Add(g.Node);
					res = true;
				}
				if (!g.Optional) break;
			}
			return res;
		}

		protected void Merge(ArrayList a, ArrayList list)
		{
			foreach (object o in a)
			{
				//Console.WriteLine(o);
				if (!list.Contains(o)) list.Add(o);
			}
		}

		protected bool _PartialMatch(IAttrTree targ, int i_targ, string[] patt, int i_patt, ArrayList result)
		{
			bool pend = patt.Length <= i_patt;
			if (targ.Count <= i_targ) return pend;
			if (pend)
			{
				return _Next(targ, i_targ, result);
			}
			IAttrTree goal = (IAttrTree)targ[i_targ++];
			string word = patt[i_patt];
			// word
			if (goal.Node.Equals(word))
			{
				return _PartialMatch(targ, i_targ, patt, i_patt + 1, result);
			}
			bool res = goal.Optional && _PartialMatch(targ, i_targ, patt, i_patt, result);
			// grammar
			if (!_grammars.Contains(goal.Node)) return false;
			IAttrTree grm = (IAttrTree)_grammars[goal.Node];
			// parse all the alternatives
			foreach (IAttrTree alt in grm)
			{
				// concatenate clause and the rest of target (after goal) to the new tree
				IAttrTree temp_targ = new AttrTree(alt);
				for (int j = i_targ; j < targ.Count; j++) temp_targ.Add(targ[j]);
				// try to match
				if (_PartialMatch(temp_targ, 0, patt, i_patt, result)) res = true;
			}
			return res;
		}

	}
}
