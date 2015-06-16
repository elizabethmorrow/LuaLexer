﻿// Elizabeth Morrow 
// Lua Lexer
//TODO: peek 2 extension method, "skipped" tokens add to trivia rather than having unknown token

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lexer
{
    class Lexer
    {
        private static readonly HashSet<string> _keywords = new HashSet<string>
        {
            "and",
            "break",
            "do",
            "else",
            "elseif",
            "end",
            "false",
            "for",
            "function",
            "goto",
            "if",
            "in",
            "local",
            "nil",
            "not",
            "or",
            "repeat",
            "return",
            "then",
            "true",
            "until",
            "while"
        };

        private static readonly Dictionary<string, Token.TokenType> _symbols = new Dictionary<string, Token.TokenType>()
        {
            { "-", Token.TokenType.OPERATOR },
            { "~", Token.TokenType.OPERATOR },
            { "#",Token.TokenType.OPERATOR },
            {"~=",Token.TokenType.OPERATOR },
            {"<=",Token.TokenType.OPERATOR },
            {">=",Token.TokenType.OPERATOR },
            {"==",Token.TokenType.OPERATOR },
            {"+",Token.TokenType.OPERATOR },
            {"*",Token.TokenType.OPERATOR },
            {"/",Token.TokenType.OPERATOR },
            {"//",Token.TokenType.OPERATOR },
            {"^",Token.TokenType.OPERATOR },
            {"%",Token.TokenType.OPERATOR },
            {"&",Token.TokenType.OPERATOR },
            {"|",Token.TokenType.OPERATOR },
            {">>",Token.TokenType.OPERATOR },
            {"<<",Token.TokenType.OPERATOR },
            {"..",Token.TokenType.OPERATOR },
            {">",Token.TokenType.OPERATOR },
            {"<", Token.TokenType.OPERATOR },
            {"=", Token.TokenType.OPERATOR },

            {"{", Token.TokenType.BRACKET_OPEN },
            {"}", Token.TokenType.BRACKET_CLOSE },
            {"(", Token.TokenType.BRACKET_OPEN },
            {")", Token.TokenType.BRACKET_CLOSE },
            {"[", Token.TokenType.BRACKET_OPEN },
            {"]", Token.TokenType.BRACKET_CLOSE },

            {".", Token.TokenType.PUNCTUATION},
            {",", Token.TokenType.PUNCTUATION},
            {";", Token.TokenType.PUNCTUATION},
            {":", Token.TokenType.PUNCTUATION},
            {"::", Token.TokenType.PUNCTUATION}
        };

        private const char EOF = unchecked((char)-1);
        private readonly char[] longCommentID1 = { '-', '[','['};
        private readonly char[] longCommentID2 = { '-', '[', '=' };

        public List<Token> Tokenize(StreamReader stream)
        {
            List<Token> tokens = new List<Token>();
            Token nextToken;
            List<Trivia> trivia;

            while (!stream.EndOfStream)
            {
                // use var (?)
                trivia = ConsumeTrivia(stream);
                
                // TODO: return longest string of acceptable values (124fut return number 124)
                nextToken = ReadNextToken(stream, trivia);

                // Add to Token List
                tokens.Add(nextToken);
            }

            // TODO: IEnum yield return
            return tokens;
        } 

        private Token ReadNextToken(StreamReader stream, List<Trivia> trivia)
        {
            if (stream.EndOfStream)
            {
                return new Token(Token.TokenType.UNKNOWN, " ", trivia);
            }

            char nextChar = (char)stream.Peek();

            // Keyword or Identifier
            if (char.IsLetter(nextChar) || (nextChar == '_'))
            {
                return readAlphaToken(stream, trivia);
            }
            // Number
            else if (char.IsDigit(nextChar))
            {
               return ReadNumberToken(stream, trivia);
            }
            // String
            else if (isQuote(nextChar))
            {
                return ReadStringToken(stream, trivia);
            }
            // Punctuation Bracket Operator
            else
            {
                return ReadSymbolToken(stream, trivia);
            } 
        }

        private Token readAlphaToken(StreamReader stream, List<Trivia> trivia)
        {
            // Keyword or Identifier
            char nextChar;
            StringBuilder word = new StringBuilder();
            
            do
            {
                word.Append((char)stream.Read());
                nextChar = (char)stream.Peek();

            } while (isValidAlpha(nextChar));

            string value = word.ToString();

            if (_keywords.Contains(value))
            {
                return new KeyWordToken(Token.TokenType.KEYWORD, value, trivia);
            }
            else
            {
                return new IdentifierToken(Token.TokenType.IDENTIFIER, value, trivia);
            }

        }
        
        private Token ReadNumberToken(StreamReader stream, List<Trivia> trivia)
        {
            StringBuilder number = new StringBuilder();
            char next = (char)stream.Peek();
            // bool hasDecimal;
            // bool hasExponent;
            // bool isHex;
            // TODO: verify only one decimal point

            while (IsValidNumber(next)) 
            {
                number.Append((char)stream.Read());
                next = (char)stream.Peek();
            }

            if (IsValidTerminator(next))
            {
                return new NumberToken(Token.TokenType.NUMBER, number.ToString(), trivia);
            }
            else
            {
                return new UnknownToken(Token.TokenType.NUMBER, number.ToString(), trivia);
            }
        }

        private Token ReadStringToken(StreamReader stream, List<Trivia> leadingTrivia)
        {
            StringBuilder fullString = new StringBuilder();
            char nextChar = (char)stream.Read();

            switch (nextChar)
            {
                case '"':
                    do
                    {
                        fullString.Append((char)stream.Read());
                        nextChar = (char)stream.Peek();

                    } while (nextChar != '"' && !stream.EndOfStream);

                    if (nextChar == '"')
                    {
                        fullString.Append((char)stream.Read());
                        return new StringToken(Token.TokenType.STRING, fullString.ToString(), leadingTrivia);
                    }
                    else
                    {
                        return new UnknownToken(Token.TokenType.STRING, fullString.ToString(), leadingTrivia);
                    }
                case '\'':
                    do
                    {
                        fullString.Append((char)stream.Read());
                        nextChar = (char)stream.Peek();

                    } while ((nextChar != '\'') && (!stream.EndOfStream));

                    if (nextChar == '\'')
                    {
                        return new StringToken(Token.TokenType.STRING, fullString.ToString(), leadingTrivia);
                    }
                    else
                    {
                        return new UnknownToken(Token.TokenType.STRING, fullString.ToString(), leadingTrivia);
                    }
                default:
                    return new UnknownToken(Token.TokenType.STRING, fullString.ToString(), leadingTrivia);
            }
        }

        private Token ReadSymbolToken(StreamReader stream, List<Trivia> leadingTrivia)
        {
            StringBuilder symbol = new StringBuilder();
            string fullSymbol;
            char nextChar = (char)stream.Read();

            while (char.IsSymbol(nextChar))
            {
                symbol.Append(nextChar);
                nextChar = (char)stream.Peek();
            }

            fullSymbol = symbol.ToString();

            if (_symbols.ContainsKey(fullSymbol))
            {
                return new Token(_symbols[fullSymbol], fullSymbol, leadingTrivia);
            }
            else
            {
                return new UnknownToken(Token.TokenType.UNKNOWN, fullSymbol, leadingTrivia);
            }

        }

        private bool isValidAlpha(char a)
        {
            if(Char.IsLetter(a) || Char.IsNumber(a) || (a == '_')){
                return true;
            }
            else
                return false;
        }

        private List<Trivia> ConsumeTrivia(StreamReader stream)
        {
            List<Trivia> triviaList = new List<Trivia>();
            bool isTrivia = false;

            char next;

            do
            {
                next = (char)stream.Peek();
                
                switch (next)
                {
                    case ' ':
                    case '\t':
                        isTrivia = true;
                        triviaList.Add(CollectWhitespace(stream));
                        break;

                    case '\n':
                        isTrivia = true;
                        Trivia newLineTrivia = new Trivia(Trivia.TriviaType.NEWLINE, next.ToString());
                        triviaList.Add(newLineTrivia);
                        break;

                    case '\r':
                        isTrivia = true;
                        stream.Read();
                        next = (char)stream.Peek();

                        if (next == '\n')
                        {
                            stream.Read();
                        }

                        Trivia returnTrivia = new Trivia(Trivia.TriviaType.NEWLINE, Environment.NewLine);
                        triviaList.Add(returnTrivia);
                        break;

                    case '-':
                        stream.Read();

                        if((char)stream.Peek() == '-')
                        {
                            isTrivia = true;

                            char[] currentCommentID = new char[longCommentID1.Length];

                            int charsRead = stream.Read(currentCommentID, 0, longCommentID1.Length);

                            if (currentCommentID.SequenceEqual(longCommentID1) || (currentCommentID.SequenceEqual(longCommentID2)))
                            {
                                triviaList.Add(readLongComment(stream, currentCommentID));
                            }
                            else
                            {
                                triviaList.Add(readLineComment(stream, currentCommentID));
                            }
                        }
                        else
                        {
                            isTrivia = false;
                            
                            //TODO: Write extension method to peek 2 ahead
                            
                            stream.BaseStream.Position--;
                        }
                        break;

                    default:
                        isTrivia = false;
                        break;
                }

            } while (isTrivia);

            return triviaList;
        }

        private Trivia CollectWhitespace(StreamReader stream)
        {
            StringBuilder whitespace = new StringBuilder();
            whitespace.Append((char)stream.Read());

            while (char.IsWhiteSpace((char)stream.Peek()))
            {
                whitespace.Append((char)stream.Read());
            }

            return new Trivia(Trivia.TriviaType.WHITESPACE, whitespace.ToString());
        }
        
        bool IsValidTerminator(char next)
        {
            switch(next)
            {
                case ';':
                case ' ':
                case '\n':
                case '\t':
                case EOF:
                    return true;
                default:
                    return false;
            }
        }

        bool isQuote(char nextChar)
        {
            return ((nextChar == '"') || (nextChar == '\''));
        }

        Trivia readLineComment(StreamReader stream, char[] commentRead)
        {
            string comment = new string(commentRead);
            comment += stream.ReadLine();
            return new Trivia(Trivia.TriviaType.COMMENT, string.Concat("-", comment));
        }

        Trivia readLongComment(StreamReader stream, char[] commentRead)
        {
            StringBuilder comment = new StringBuilder();
            comment.Append("-").Append(commentRead);

            int level = 0;
            char next;

            switch (commentRead[(commentRead.Length - 1)])
            {
                case '=':
                    level++;
                    next = (char)stream.Peek();

                    // Get levels (=) 
                    while(next == '=')
                    {
                        comment.Append((char)stream.Read());
                        level++;
                        next = (char)stream.Peek();
                    }

                    if(next == '[')
                    {
                        comment.Append((char)stream.Read());

                        while(level != 0)
                        {
                            next = (char)stream.Peek();

                            if (next == ']')
                            {
                                comment.Append((char)stream.Read());
                                int currentLevel = level;
                                next = (char)stream.Peek();

                                while((next == '=') && (currentLevel > 0))
                                {
                                    comment.Append((char)stream.Read());
                                    currentLevel--;
                                    next = (char)stream.Peek();
                                }

                                if((next == ']') && (currentLevel == 0))
                                {
                                    comment.Append((char)stream.Read());
                                    level = 0;
                                    return new Trivia(Trivia.TriviaType.COMMENT, comment.ToString());
                                }
                                
                                // TODO: speed this up, fix the condition in the while (bool commentClosed?)
                            }
                            else
                            {
                                comment.Append((char)stream.Read());
                            }
                        } 
                    }
                    else
                    {
                        // TODO: fix that double type cast
                        char[] alreadyConsumed = comment.ToString().ToCharArray();

                        return readLineComment(stream, alreadyConsumed);
                    }

                    break;
                case '[':
                    comment.Append((char)stream.Read());
                    char[] validEnd = { ']', ']' };
                    char[] currentEnd = { };
                    stream.Read(currentEnd, 0, validEnd.Length);

                    while(currentEnd != validEnd)
                    {
                        comment.Append(currentEnd);
                        stream.Read(currentEnd, 0, validEnd.Length);
                    }

                    comment.Append(currentEnd);
                    return new Trivia(Trivia.TriviaType.COMMENT, comment.ToString());
            }

            return new Trivia(Trivia.TriviaType.COMMENT, comment.ToString());
        }

        bool IsValidNumber(char next)
        {
            return (char.IsDigit(next) || (next == '.') || (next == 'e') || (next == 'x'));
        }
    }
}
