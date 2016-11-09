using System;
using System.IO;

namespace MoreExtras.SimpleParser
{

	public class SemTree : AttrTree
	{
		public class AttrFormatException : FormatException
		{
			public AttrFormatException(AttrTree Attributes, AttrTree cat)
				: base(cat.ToString())
			{
				// find "file" and set Source field
				AttrTree file = find(Attributes, new string[] { "file" });
				if (file != null && file.Count > 0)
				{
					AttrTree tf = ((AttrTree)file[0]);
					this.Source = tf.Node;
					if (tf.Count > 0)
					{
						AttrTree tl = ((AttrTree)tf[0]);
						int line = int.Parse(tl.Node);
					}
				}
			}
		}
		// constructors
		public SemTree() : base() { }
		public SemTree(string node, AttrTree attributes)
		{
			Node = node;
			Attributes = attributes;
		}
		public SemTree(AttrTree at)
		{
			Node = at.Node;
			Attributes = at.Attributes;
			IsOptional = at.IsOptional;
			foreach (var t in at) Add(t);
		}
		public SemTree(TextReader t) : base(t) { }
		// ISemTree implementation
		public string Eval(string cat)
		{
			string op = null, nv = "";
			switch (cat)
			{
				case Constants.Semantics.NUMBER:
					op = null;
					nv = "0";
					break;
				case Constants.Semantics.STRING:
					op = " ";
					nv = null;
					break;
			}
			return Eval(cat, op, nv, "", "");
		}
		public string Eval(string cat, string op, string nv)
		{
			return Eval(cat, op, nv, "", "");
		}
		public string Eval(string cat, string op, string nv, string ld, string rd)
		{
			if (Attributes != null)
			{
				AttrTree _alias = find(Attributes, new string[] { Constants.Semantics.ALIAS, cat });
				if (_alias != null)
				{
					if (_alias.Count != 1) throw new AttrFormatException(Attributes, _alias);
					return Eval(((AttrTree)_alias[0]).Node, op, nv, ld, rd);
				}
				AttrTree _value = find(Attributes, new string[] { Constants.Semantics.VALUE, cat });
				if (_value != null)
				{
					if (_value.Count > 1) throw new AttrFormatException(Attributes, _value);
					return ((AttrTree)_value[0]).Node;
				}
				AttrTree _oprtr = find(Attributes, new string[] { Constants.Semantics.OPRTR, cat });
				if (_oprtr != null)
				{
					if (_oprtr.Count != 1) throw new AttrFormatException(Attributes, _oprtr);
					op = ((AttrTree)_oprtr[0]).Node;
				}
			}
			if (op == null) op = "";
			if (Count == 0)
			{
				if (nv != null) return nv;
				//				int p = Node.IndexOf(".");
				//				if(p >= 0) return Node.Substring(0,p);
				return Node;
			}
			string v1 = null;
			foreach (SemTree t in this)
			{
				string v2 = t.Eval(cat, op, nv, ld, rd);
				if (v1 != null && v1.Length > 0 && v2.Length > 0)
				{
					v1 = _binop(op, v1, v2);
				}
				else if (v2.Length > 0)
				{
					v1 = v2;
				}
			}
			return ld + v1 + rd;
		}
		// helping stuff
		static protected AttrTree find(AttrTree tree, string[] list)
		{
			return find(tree, list, 0);
		}
		static protected AttrTree find(AttrTree tree, string[] list, int n)
		{
			foreach (AttrTree t in tree)
			{
				if (t.Node == list[n])
					return n < list.Length - 1 ? find(t, list, n + 1) : t;
			}
			return null;
		}
		protected string _binop(string op, string l, string r)
		{
			if (op == null || op.Length == 0) return l + r;
			string v = _arithm(op, l, r);
			if (v != null && v.Length > 0) return v;
			return l + op + r;
		}
		protected string _arithm(string op, string l, string r)
		{
			try
			{
				double dl = Double.Parse(l), dr = Double.Parse(r), res = 0;
				switch (op)
				{
					case "+": res = dl + dr; break;
					case "-": res = dl - dr; break;
					case "*": res = dl * dr; break;
					case "/": res = dl / dr; break;
					case "%": res = dl % dr; break;
					default: return null;
				}
				return string.Format("{0:g}", res);
			}
			catch (Exception)
			{
			}
			return null;
		}
	}
}
