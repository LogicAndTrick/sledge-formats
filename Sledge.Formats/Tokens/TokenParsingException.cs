using System;

namespace Sledge.Formats.Tokens
{
    public class TokenParsingException : Exception
    {
        public Token Token { get; }

        public TokenParsingException(Token tok, string message, Exception innerException = null) : base(GetMessage(tok, message), innerException)
        {
            Token = tok;
        }

        private static string GetMessage(Token tok, string message)
        {
            var msg = "Parsing error";

            if (tok != null)
            {
                msg += $" (line {tok.Line}, column {tok.Column})";
            }

            if (!String.IsNullOrEmpty(message))
            {
                msg += ": " + message;
            }

            return msg;
        }
    }
}