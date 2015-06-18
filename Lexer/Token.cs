using System;
using System.Collections.Generic;

namespace Lexer
{
    internal class Token
    {
        public enum TokenType { Keyword, Operator, Identifier, Punctuation, BracketOpen, BracketClose, Number, String, UNKNOWN, EOF }

        protected int fullStart;
        protected int start;
        protected int length;
        protected string value;
        protected TokenType type;
        protected List<Trivia> leadingTrivia;


        public Token(TokenType tokentype, string value, List<Trivia> trivia)
        {
            this.type = tokentype;
            this.leadingTrivia = trivia;
            this.value = value;
        }

        // for debugging only
        public void PrintType()
        {
            Console.Write(this.type.ToString() + ": ");
            Console.WriteLine(value);
        }

        public void PrintTrivia()
        {
            foreach(Trivia trivia in leadingTrivia)
            {
                Console.Write("Trivia: " + trivia.trivia);
            }
        }
    }

    // Currently subclasses not needed, might need as data model evolves

    /*public class KeywordToken : Token
    {
        public KeywordToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
            this._value = value;
        }

    }

    public class StringToken : Token
    {
        public StringToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
        }

    }

    public class UnknownToken : Token
    {
        public UnknownToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
        }

    }

    public class NumberToken : Token
    {
        public NumberToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
        }
    }

    public class IdentifierToken : Token
    {
        public IdentifierToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
        }

    }*/
}
