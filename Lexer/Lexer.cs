// Elizabeth Morrow 
// Lua Lexer
//TODO: peek 2 extension method
// "skipped" tokens add to trivia rather than having unknown token, 
// jump tables for is valid character of any kind, whitespace, digit, letter/underscore
// use stream not streamreader

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lexer
{
    internal class Lexer
    {
        private static readonly HashSet<string> Keywords = new HashSet<string>
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

        private static readonly Dictionary<string, Token.TokenType> Symbols = new Dictionary<string, Token.TokenType>
        {
            { "-", Token.TokenType.Operator },
            { "~", Token.TokenType.Operator },
            { "#", Token.TokenType.Operator },
            {"~=", Token.TokenType.Operator },
            {"<=", Token.TokenType.Operator },
            {">=", Token.TokenType.Operator },
            {"==", Token.TokenType.Operator },
            {"+", Token.TokenType.Operator },
            {"*", Token.TokenType.Operator },
            {"/", Token.TokenType.Operator },
            {"//", Token.TokenType.Operator },
            {"^", Token.TokenType.Operator },
            {"%", Token.TokenType.Operator },
            {"&", Token.TokenType.Operator },
            {"|", Token.TokenType.Operator },
            {">>", Token.TokenType.Operator },
            {"<<", Token.TokenType.Operator },
            {"..", Token.TokenType.Operator },
            {">", Token.TokenType.Operator },
            {"<", Token.TokenType.Operator },
            {"=", Token.TokenType.Operator },

            {"{", Token.TokenType.BracketOpen },
            {"}", Token.TokenType.BracketClose },
            {"(", Token.TokenType.BracketOpen },
            {")", Token.TokenType.BracketClose },
            {"[", Token.TokenType.BracketOpen },
            {"]", Token.TokenType.BracketClose },

            {".", Token.TokenType.Punctuation},
            {",", Token.TokenType.Punctuation},
            {";", Token.TokenType.Punctuation},
            {":", Token.TokenType.Punctuation},
            {"::", Token.TokenType.Punctuation}
        };

        private const char EOF = unchecked((char)-1);
        private readonly char[] longCommentID1 = { '-', '[','[' };
        private readonly char[] longCommentID2 = { '-', '[', '=' };

        public List<Token> Tokenize(StreamReader stream)
        {
            List<Token> tokens = new List<Token>();
            Token nextToken;
            List<Trivia> trivia;

            while (!stream.EndOfStream)
            {
                trivia = this.ConsumeTrivia(stream);
                
                // TODO: return longest string of acceptable values (124fut return number 124)
                nextToken = this.ReadNextToken(stream, trivia);
                
                tokens.Add(nextToken);
            }
            // check here for eof?
            // TODO: IEnum yield return
            return tokens;
        } 

        private Token ReadNextToken(StreamReader stream, List<Trivia> trivia)
        {
            if (stream.EndOfStream)
            {
                return new Token(Token.TokenType.EOF, "", trivia);
            }

            char nextChar = (char)stream.Peek();

            // Keyword or Identifier
            if (char.IsLetter(nextChar) || (nextChar == '_'))
            {
                return this.ReadAlphaToken(stream, trivia);
            }
            // Number
            else if (char.IsDigit(nextChar))
            {
               return this.ReadNumberToken(stream, trivia);
            }
            // String
            else if (IsQuote(nextChar))
            {
                return this.ReadStringToken(stream, trivia);
            }
            // Punctuation Bracket Operator
            else
            {
                return this.ReadSymbolToken(stream, trivia);
            } 
        }

        private Token ReadAlphaToken(StreamReader stream, List<Trivia> trivia)
        {
            // Keyword or Identifier
            char nextChar;
            StringBuilder word = new StringBuilder();
            
            do
            {
                word.Append((char)stream.Read());
                nextChar = (char)stream.Peek();
            } while (this.IsAlphaCharacter(nextChar));

            string value = word.ToString();

            if (Keywords.Contains(value))
            {
                return new Token(Token.TokenType.Keyword, value, trivia);
            }
            else
            {
                return new Token(Token.TokenType.Identifier, value, trivia);
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

            while (this.IsValidNumber(next)) 
            {
                number.Append((char)stream.Read());
                next = (char)stream.Peek();
            }

            if (this.IsValidTerminator(next))
            {
                return new Token(Token.TokenType.Number, number.ToString(), trivia);
            }
            else
            {
                return new Token(Token.TokenType.UNKNOWN, number.ToString(), trivia);
            }
        }

        private Token ReadStringToken(StreamReader stream, List<Trivia> leadingTrivia)
        {
            StringBuilder fullString = new StringBuilder();
            char nextChar = (char)stream.Peek(); 

            switch (nextChar)
            {
                case '"':
                    do
                    {
                        fullString.Append((char)stream.Read());
                        nextChar = (char)stream.Peek();

                    } while ((nextChar != '"') && !stream.EndOfStream);

                    if (nextChar == '"')
                    {
                        fullString.Append((char)stream.Read());
                        return new Token(Token.TokenType.String, fullString.ToString(), leadingTrivia);
                    }
                    else
                    {
                        return new Token(Token.TokenType.UNKNOWN, fullString.ToString(), leadingTrivia);
                    }
                case '\'':
                    do
                    {
                        fullString.Append((char)stream.Read());
                        nextChar = (char)stream.Peek();

                    } while ((nextChar != '\'') && (!stream.EndOfStream));

                    if (nextChar == '\'')
                    {
                        return new Token(Token.TokenType.String, fullString.ToString(), leadingTrivia);
                    }
                    else
                    {
                        return new Token(Token.TokenType.UNKNOWN, fullString.ToString(), leadingTrivia);
                    }
                // case '[':
                default:
                    fullString.Append((char)stream.Read());
                    bool terminated = false;
                    switch ((char)stream.Peek())
                    {
                        case '[':
                            fullString.Append((char)stream.Read());

                            nextChar = (char)stream.Peek();

                            while (!terminated && !stream.EndOfStream)
                            {
                                if(nextChar == ']')
                                {
                                    fullString.Append((char)stream.Read());
                                    nextChar = (char)stream.Peek();
                                    if (nextChar == ']')
                                    {
                                        fullString.Append((char)stream.Read());
                                        terminated = true;
                                    }
                                    else
                                    {
                                        fullString.Append((char)stream.Read());
                                        nextChar = (char)stream.Peek();
                                    }

                                }
                                else
                                {
                                    fullString.Append((char)stream.Read());
                                    nextChar = (char)stream.Peek();
                                }
                            }
                            return new Token(Token.TokenType.String, fullString.ToString(), leadingTrivia);
                        case '=':
                            fullString.Append((char)stream.Read());
                            int level = 1;
                            
                            nextChar = (char)stream.Peek();

                            // Get levels (=) 
                            while (nextChar == '=')
                            {
                                fullString.Append((char)stream.Read());
                                level++;
                                nextChar = (char)stream.Peek();
                            }

                            if(nextChar == '[')
                            {
                                fullString.Append((char)stream.Read());
                                nextChar = (char)stream.Peek();

                                while (!terminated && !stream.EndOfStream)
                                {
                                    if(nextChar == ']')
                                    {
                                        fullString.Append((char)stream.Read());
                                        nextChar = (char)stream.Peek();
                                        int currentLevel = level;

                                        while(nextChar == '=')
                                        {
                                            fullString.Append((char)stream.Read());
                                            level--;
                                            nextChar = (char)stream.Peek();
                                        }

                                        if((nextChar == ']') && (level == 0) )
                                        {
                                            fullString.Append((char)stream.Read());
                                            return new Token(Token.TokenType.String, fullString.ToString(), leadingTrivia);
                                        }
                                    }
                                    else
                                    {
                                        fullString.Append((char)stream.Read());
                                    }
                                    nextChar = (char)stream.Peek();
                                }

                                return new Token(Token.TokenType.String, fullString.ToString(), leadingTrivia);

                            }
                            else
                            {
                                // Error, not valid syntax
                                return new Token(Token.TokenType.UNKNOWN, fullString.ToString(), leadingTrivia);
                            }
                        default:
                            return new Token(Token.TokenType.BracketOpen, nextChar.ToString(), leadingTrivia);
                    }
            }
        }

        private Token ReadSymbolToken(StreamReader stream, List<Trivia> leadingTrivia)
        {
            char nextChar = (char)stream.Read();

            switch (nextChar)
            {
                case ':':
                case '.':
                    // here use dictionary for minux, plus etc
                    if(nextChar != (char)stream.Peek())
                    {
                        return new Token(Token.TokenType.Punctuation, nextChar.ToString(), leadingTrivia);
                    }
                    else
                    {
                        char[] symbol = { nextChar, nextChar };
                        return new Token(Token.TokenType.Punctuation, symbol.ToString(), leadingTrivia);
                    }
                case '<':
                case '>':
                    // could be doubles or eq sign
                    if ((nextChar != (char)stream.Peek()) && ((char)stream.Peek()!= '='))
                    {
                        return new Token(Token.TokenType.Operator, nextChar.ToString(), leadingTrivia);
                    }
                    else
                    {
                        char secondOperatorChar = (char)stream.Read();
                        char[] symbol = { nextChar, secondOperatorChar };
                        return new Token(Token.TokenType.Operator, symbol.ToString(), leadingTrivia);
                    }
                case '=':
                case '/':
                    if (nextChar != (char)stream.Peek())
                    {
                        return new Token(Token.TokenType.Operator, nextChar.ToString(), leadingTrivia);
                    }
                    else
                    {
                        char[] symbol = { nextChar, nextChar };
                        return new Token(Token.TokenType.Operator, symbol.ToString(), leadingTrivia);
                    }
                case '~':
                    if ((char)stream.Peek() != '=')
                    {
                        return new Token(Token.TokenType.Operator, nextChar.ToString(), leadingTrivia);
                    }
                    else
                    {
                        char[] symbol = { nextChar, '=' };
                        return new Token(Token.TokenType.Operator, symbol.ToString(), leadingTrivia);
                    }
                default:
                    // non repeating symbol
                    string fullSymbol = nextChar.ToString();
                    if (Symbols.ContainsKey(fullSymbol))
                    {
                        return new Token(Symbols[fullSymbol], fullSymbol, leadingTrivia);
                    }
                    else
                    {
                        return new Token(Token.TokenType.UNKNOWN, fullSymbol, leadingTrivia);
                    }
            }
        }

        private bool IsAlphaCharacter(char a)
        {
            return (char.IsLetter(a) || char.IsNumber(a) || (a == '_'));
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
                        Trivia newLineTrivia = new Trivia(Trivia.TriviaType.Newline, next.ToString());
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

                        Trivia returnTrivia = new Trivia(Trivia.TriviaType.Newline, Environment.NewLine);
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
                                triviaList.Add(ReadLongComment(stream, currentCommentID));
                            }
                            else
                            {
                                triviaList.Add(ReadLineComment(stream, currentCommentID));
                            }
                        }
                        else
                        {
                            isTrivia = false;
                            
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

            return new Trivia(Trivia.TriviaType.Whitespace, whitespace.ToString());
        }
        
        bool IsValidTerminator(char next)
        {
            switch(next)
            {
                case ';':
                case ' ':
                case '\n':
                case '\t':
                case Lexer.EOF:
                    return true;
                default:
                    return false;
            }
        }

        bool IsQuote(char nextChar)
        {
            return ((nextChar == '"') || (nextChar == '\'') || (nextChar == '['));
        }

        Trivia ReadLineComment(StreamReader stream, char[] commentRead)
        {
            string comment = "-" + commentRead;
            comment += stream.ReadLine();
            return new Trivia(Trivia.TriviaType.Comment, comment);
        }

        Trivia ReadLongComment(StreamReader stream, char[] commentRead)
        {
            StringBuilder comment = new StringBuilder();
            comment.Append("-").Append(commentRead);

            int level = 0;
            char next;

            switch (commentRead[commentRead.Length - 1])
            {
                case '=':
                    level++;
                    next = (char)stream.Peek();

                    // Get levels (=) 
                    while (next == '=')
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
                                    return new Trivia(Trivia.TriviaType.Comment, comment.ToString());
                                }
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

                        return ReadLineComment(stream, alreadyConsumed);
                    }

                    break;
                case '[':
                    comment.Append((char)stream.Read());
                    char[] validEnd = { ']', ']' };
                    char[] currentEnd = new char[validEnd.Length];
                    stream.Read(currentEnd, 0, validEnd.Length);

                    while(currentEnd != validEnd)
                    {
                        comment.Append(currentEnd);
                        stream.Read(currentEnd, 0, validEnd.Length);
                    }

                    comment.Append(currentEnd);
                    return new Trivia(Trivia.TriviaType.Comment, comment.ToString());
            }

            return new Trivia(Trivia.TriviaType.Comment, comment.ToString());
        }

        bool IsValidNumber(char character)
        {
            // switch 1-9, . , e, x
            return (char.IsDigit(character) || (character == '.') || (character == 'e') || (character == 'x'));
        }
    }
}
