namespace Ron.NET;

public struct Token
{
    public TokenType TokenType;
    public object Literal;

    public Token(TokenType tokenType, object literal)
    {
        TokenType = tokenType;
        Literal = literal;
    }

    public override string ToString()
    {
        return $"{TokenType.ToString().ToUpper()}, Literal: {Literal}";
    }
}