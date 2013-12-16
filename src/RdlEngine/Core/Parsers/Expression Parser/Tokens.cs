/*
 * This file is part of UniERM ReportDesigner, based on reportFU by Josh Wilson,
 * the work of Kim Sheffield and the fyiReporting project. 
 * 
 * Prior Copyrights:
 * _________________________________________________________
 * |Copyright (C) 2010 devFU Pty Ltd, Josh Wilson and Others|
 * | (http://reportfu.org)                                  |
 * =========================================================
 * _________________________________________________________
 * |Copyright (C) 2004-2008  fyiReporting Software, LLC     |
 * |For additional information, email info@fyireporting.com |
 * |or visit the website www.fyiReporting.com.              |
 * =========================================================
 *
 * License:
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Reporting.Rdl
{
    /// <summary>
    /// Summary description for TokenTypes.
    /// </summary>
    internal enum TokenTypes
    {
        AND,
        OR,
        NOT,
        PLUS,
        PLUSSTRING,
        MINUS,
        LPAREN,
        RPAREN,
        QUOTE,
        IDENTIFIER,
        COMMA,
        NUMBER,
        DATETIME,
        DOUBLE,
        INTEGER,
        EQUAL,
        NOTEQUAL,
        GREATERTHAN,
        GREATERTHANOREQUAL,
        LESSTHAN,
        LESSTHANOREQUAL,
        FORWARDSLASH,
        BACKSLASH,
        STAR,
        EXP,
        MODULUS,
        DOT,                // dot operator
        OTHER,
        EOF
    }

	/// <summary>
	/// Token class that used by LangParser.
	/// </summary>
	internal class Token
	{
		internal string Value;
		internal int StartLine;
		internal int EndLine;
		internal int StartCol;
		internal int EndCol;
		internal TokenTypes Type;

		/// <summary>
		/// Initializes a new instance of the Token class.
		/// </summary>
        
		internal Token(string value, int startLine, int startCol, int endLine, int endCol, TokenTypes type)
		{
			Value = value;
			StartLine = startLine;
			EndLine = endLine;
			StartCol = startCol;
			EndCol = endCol;
			Type = type;
		}

		/// <summary>
		/// Initializes a new instance of the Token class.
		/// </summary>
		internal Token(string value, TokenTypes type)
			: this(value, 0, 0, 0, 0, type)
		{
			// use this
		}

		/// <summary>
		/// Initializes a new instance of the Token class.
		/// </summary>
		internal Token(TokenTypes type)
			: this(null, 0, 0, 0, 0, type)
		{
			// use this
		}

		/// <summary>
		/// Returns a string representation of the Token.
		/// </summary>
		public override string ToString()
		{
			return "<" + Type + "> " + Value;	
		}
	}

    /// <summary>
    /// Represents a list of the tokens.
    /// </summary>
    internal class TokenList : IEnumerable
    {
        private List<Token> tokens = null;

        [System.Diagnostics.DebuggerStepThrough]
        internal TokenList()
        {
            tokens = new List<Token>();
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal void Add(Token token)
        {
            tokens.Add(token);
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal void Push(Token token)
        {
            tokens.Insert(0, token);
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal Token Peek()
        {
            return tokens[0];
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal Token Extract()
        {
            Token token = tokens[0];
            tokens.RemoveAt(0);
            return token;
        }

        internal int Count
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return tokens.Count;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerator GetEnumerator()
        {
            return tokens.GetEnumerator();
        }
    }
}
