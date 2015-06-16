using System;
using System.Collections.Generic;

namespace Lexer
{
    public class Token
    {
        public enum TokenType { KEYWORD, OPERATOR, IDENTIFIER, PUNCTUATION, BRACKET_OPEN, BRACKET_CLOSE, NUMBER, STRING, UNKNOWN }

        protected int _fullStart;
        protected int _start;
        protected int _length;
        string _value;
        protected TokenType _type;
        protected List<Trivia> _leadingTrivia;


        public Token(TokenType tokentype, string value, List<Trivia> trivia)
        {
            _type = tokentype;
            _leadingTrivia = trivia;
            _value = value;
        }

        // for debugging only
        public void printType()
        {
            Console.WriteLine(_type.ToString());
        }
    }

    // Currently subclasses not needed, might need as data model evolves

    public class KeyWordToken : Token
    {
        public KeyWordToken(TokenType tokentype, string value, List<Trivia> trivia) : base(tokentype, value, trivia)
        {
            // Initialize
            this._type = tokentype;
            this._leadingTrivia = trivia;
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

    }
}
