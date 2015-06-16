using System;
using System.Collections.Generic;
using System.IO;

namespace Lexer
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "C:/Users/t-elmorr/Documents/LuaTests/testLM.lua"; // TODO

            if (filePath == null)
            {
                Console.WriteLine("Please Select a File");
                return;
            }

            // read in stream, next token
            using (StreamReader stream = new StreamReader(filePath))
            {
                Lexer lexer = new Lexer();

                List<Token> tokenList = lexer.Tokenize(stream); // return IEnum

                foreach (Token token in tokenList)
                {
                    token.printType();
                }

                // For debugging purposes

                Console.Read();
            }
        }
    }
}
