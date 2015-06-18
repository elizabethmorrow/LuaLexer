namespace Lexer
{
    public class Trivia
    {
        public enum TriviaType { WHITESPACE, COMMENT, NEWLINE} // TODO: skippedtoken

        protected TriviaType type;
        public string trivia;

        public Trivia(TriviaType type, string trivia)
        {
            // initialize
            this.type = type;
            this.trivia = trivia;
        }
    }
}
