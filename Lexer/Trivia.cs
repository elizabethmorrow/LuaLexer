namespace Lexer
{
    public class Trivia
    {
        public enum TriviaType { WHITESPACE, COMMENT, NEWLINE} // TODO: skippedtoken

        TriviaType _type;
        string _trivia;

        public Trivia(TriviaType type, string trivia)
        {
            // initialize
            _type = type;
            _trivia = trivia;
        }
    }
}
