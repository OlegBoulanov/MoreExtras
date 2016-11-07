using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;

namespace Eliza.Net {

	/// <summary>
	/// Attributed tree class
	/// </summary>
	public class AttrTree : ArrayList, IAttrTree {
		protected string _node;
		protected bool _opt;
		protected IAttrTree _attr;
		public string Node { get { return _node; } set { _node = value; } }
		public bool Optional { get { return _opt; } set { _opt = value; } }																	    
		public IAttrTree Attr { get { return _attr; } set { _attr = (AttrTree) value; } }
		// IAttrTree implementation
		public AttrTree() {
			_node = null;
			_attr = null;
			_opt = false;
		}
		public AttrTree(IAttrTree at) {
			_node = at.Node;
			_attr = at.Attr;
			_opt = at.Optional;
			foreach(IAttrTree t in at) Add(t);//new AttrTree(t));
		}
		public override object Clone() {
			IAttrTree tc = new AttrTree(_node,Attr!=null?(IAttrTree)Attr.Clone():null,_opt);
			foreach(IAttrTree t in this) tc.Add((IAttrTree)t.Clone());
			return tc;
		}
		public void Read(TextReader t) {
			Read(t, S_DELIMITERS + C_ATTRIBUTES, S_DELIMITERS);
		}
		public void Read(TextReader t, string dn, string da) {
			ws(t);
			if(t.Peek() == C_OPEN_OPTIONAL) {
				_opt = true; t.Read();
				_node = ReadUntil(t,S_CLOSE_OPTIONAL); ws(t);
				if(t.Read() != C_CLOSE_OPTIONAL) 
					throw new FormatException("'" + C_CLOSE_OPTIONAL + "' expected in " + _node);
			} else if(t.Peek() == C_OPTIONAL) {
				_opt = true; t.Read();
				_node = ReadUntil(t,dn);
			} else {
				_node = ReadUntil(t,dn);
			}
			ws(t);
			if(t.Peek() == C_ATTRIBUTES) {
				t.Read();
				AttrTree attr = new AttrTree();
				attr.Read(t,dn,da);
				_attr = attr;
			}
			if(t.Peek() == C_OPEN) {
				do {
					t.Read();
					AttrTree _tree = new AttrTree();
					_tree.Read(t,dn,da);
					Add(_tree);
				} while(t.Peek() == C_SEPARATOR);
				if(t.Read() != C_CLOSE)
					throw new FormatException("'" + C_CLOSE + "' expected in " + _node);
			}
			ws(t);
		}
		protected string ReadUntil(TextReader t, string delim) { 
			StringBuilder sb = new StringBuilder();
			ws(t);
			if(S_QUOTES.IndexOf((char)t.Peek()) >= 0) {
				for(int c, d = t.Read(); (c = t.Read()) != d; ) {
					if(c == -1) throw new FormatException("Closing " + (char)d + " is missing");
					sb.Append((char)c); 
				}
			} else {
				while(t.Peek() != -1 && ' ' < t.Peek() && delim.IndexOf((char)t.Peek()) < 0) sb.Append((char)t.Read());
			}
			return sb.ToString();
		}
		public void Write(TextWriter t) { Write(t, S_TAB, S_NEWLINE); }
		public void Write(TextWriter t, string sp) { Write(t, sp, S_NEWLINE); }
		public void Write(TextWriter t, string sp, string nl) {
			Write(t,sp,nl,sp);
		}
		protected void Write(TextWriter t, string sp, string nl, string level) {
			if(_opt) t.Write(S_OPEN_OPTIONAL);
			if(_node == null) {
			} else if(needs_quotes(_node)) {
				t.Write(C_DOUBLEQUOTE);
				t.Write(_node);
				t.Write(C_DOUBLEQUOTE);
			} else {
				t.Write(_node);
			}
			if(_opt) t.Write(S_CLOSE_OPTIONAL);
			if(_attr != null) {
				t.Write(S_ATTRIBUTES);
				_attr.Write(t,"","");
			}
			if(Count > 0) {
				if((_node != null && _node.Length > 0) || _attr != null) t.Write(" ");
				t.Write(S_OPEN);
				bool f = true;
				foreach(AttrTree st in this) {
					if(!f) t.Write(S_SEPARATOR); f = false;
					if(Count == 1 && st.Count == 0) {
						st.Write(t,sp,nl);
					} else {
						t.Write(nl);
						t.Write(level);
						st.Write(t,sp,nl,level+sp);
					}
				}
				t.Write(S_CLOSE);
			}
		}

		static protected void ws(TextReader t) { while((uint)t.Peek() <= ' ') t.Read(); }

		static readonly char [] INNER_SPECIALS = { C_SPACE, C_TAB, C_OPEN, C_CLOSE, C_SEPARATOR, C_ATTRIBUTES };
		static bool needs_quotes(string s) {
			if(s.Length == 0) return false;
			if((s[0] == C_DOUBLEQUOTE || s[0] == C_SINGLEQUOTE) && s[s.Length-1] == s[0]) return false;
			return s.IndexOfAny(INNER_SPECIALS) >= 0;
		}

		const char C_SPACE					= ' ';
		const char C_TAB					= '\t';
		const char C_NEWLINE				= '\n';
		const char C_OPEN					= '(';
		const char C_CLOSE					= ')';
		const char C_OPEN_OPTIONAL			= '[';
		const char C_CLOSE_OPTIONAL			= ']';
		const char C_OPTIONAL				= '?';
		const char C_SEPARATOR				= ',';
		const char C_ATTRIBUTES				= ':';
		const char C_DOUBLEQUOTE			= '\"';
		const char C_SINGLEQUOTE			= '\'';

		public const string S_SPACE			= " ";
		public const string S_TAB			= "\t";
		public const string S_NEWLINE		= "\n";

		const string S_OPEN					= "( ";
		const string S_CLOSE				= " )";
		const string S_OPEN_OPTIONAL		= "[";
		const string S_CLOSE_OPTIONAL		= "]";
		const string S_OPTIONAL				= "? ";
		const string S_SEPARATOR			= ", ";
		const string S_ATTRIBUTES			= " : ";
		const string S_QUOTES				= "\'\"";
		const string S_DELIMITERS			= "(,)";

		// more useful constructors
		public AttrTree(TextReader t) { Read(t); }
		//public AttrTree(string s) : this() { 
		//	//Read(new StringReader(s)); 
		//	_node = s;
		//}
		public AttrTree(string n, IAttrTree a, bool o, params IAttrTree [] sub) {
			_node = n;
			_attr = a;
			_opt = o;
			foreach(IAttrTree t in sub) Add(t);
		}
		// more stuff
		public string ToString(string sp) {
			return ToString(sp,S_NEWLINE);
		}
		public string ToString(string sp, string nl) {
			StringWriter sw = new StringWriter();
			Write(sw,sp,nl);
			return sw.ToString();
		}
		// override base method
		public override string ToString() {
			return ToString("","");
		}
	}

}
