using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MoreExtras.Parser
{
	public class AttrTree : List<AttrTree>
	{
		public string Node { get; set; } = null;
		public bool IsOptional { get; set; } = false;																    
		public AttrTree Attributes { get; set; } = null;
		public AttrTree() {}
		public AttrTree(AttrTree at) {
			Node = at.Node;
			Attributes = at.Attributes;
			IsOptional = at.IsOptional;
			foreach(var t in at) Add(t);
		}
		public void Read(TextReader t) {
			Read(t, S_DELIMITERS + C_ATTRIBUTES, S_DELIMITERS);
		}
		public void Read(TextReader t, string dn, string da) {
			ws(t);
			if(t.Peek() == C_OPEN_OPTIONAL) {
				IsOptional = true; t.Read();
				Node = ReadUntil(t,S_CLOSE_OPTIONAL); ws(t);
				if(t.Read() != C_CLOSE_OPTIONAL) 
					throw new FormatException("'" + C_CLOSE_OPTIONAL + "' expected in " + Node);
			} else if(t.Peek() == C_OPTIONAL) {
				IsOptional = true; t.Read();
				Node = ReadUntil(t,dn);
			} else {
				Node = ReadUntil(t,dn);
			}
			ws(t);
			if(t.Peek() == C_ATTRIBUTES) {
				t.Read();
				AttrTree attr = new AttrTree();
				attr.Read(t,dn,da);
				Attributes = attr;
			}
			if(t.Peek() == C_OPEN) {
				do {
					t.Read();
					AttrTree _tree = new AttrTree();
					_tree.Read(t,dn,da);
					Add(_tree);
				} while(t.Peek() == C_SEPARATOR);
				if(t.Read() != C_CLOSE)
					throw new FormatException("'" + C_CLOSE + "' expected in " + Node);
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
			if(IsOptional) t.Write(S_OPEN_OPTIONAL);
			if(Node == null) {
			} else if(needs_quotes(Node)) {
				t.Write(C_DOUBLEQUOTE);
				t.Write(Node);
				t.Write(C_DOUBLEQUOTE);
			} else {
				t.Write(Node);
			}
			if(IsOptional) t.Write(S_CLOSE_OPTIONAL);
			if(Attributes != null) {
				t.Write(S_ATTRIBUTES);
				Attributes.Write(t,"","");
			}
			if(Count > 0) {
				if((Node != null && Node.Length > 0) || Attributes != null) t.Write(" ");
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
		public AttrTree(string n, AttrTree a, bool o, params AttrTree [] sub) {
			Node = n;
			Attributes = a;
			IsOptional = o;
			foreach(AttrTree t in sub) Add(t);
		}
		// more stuff
		public string ToString(string sp) 
		{
			return ToString(sp,S_NEWLINE);
		}
		public string ToString(string sp, string nl) 
		{
			StringWriter sw = new StringWriter();
			Write(sw,sp,nl);
			return sw.ToString();
		}
		// override base method
		public override string ToString() 
		{
			return ToString("","");
		}
	}

}
