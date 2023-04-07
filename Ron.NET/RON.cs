using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ron.NET;

public static class RON
{
    private static RonGenMethods _genMethods;

    public static void Init(RonGenMethods methods)
    {
        _genMethods = methods;
    }
    
    public static IElement Parse(string ron)
    {
        RonParser parser = new RonParser(ron);

        Token[] tokens = parser.Parse();

        return ParseElement(tokens);
    }

    public static T Deserialize<T>(string ron)
    {
        return (T) _genMethods.Methods[typeof(T)].deserialize(ron);
    }

    public static string Serialize(object obj, SerializeOptions options = SerializeOptions.None)
    {
        return IElement.Serialize(_genMethods.Methods[obj.GetType()].serialize(obj), options);
    }

    private static IElement ParseElement(Token[] tokens)
    {
        IElement element = null;
        
        for (int t = 0; t < tokens.Length; t++)
        {
            ref Token token = ref tokens[t];

            switch (token.TokenType)
            {
                case TokenType.Identifier:
                    // A single token means this should be interpreted as an enum value.
                    if (tokens.Length == 1)
                    {
                        element = new ValueElement<string>((string) token.Literal, ElementType.Enum);
                        t++;
                        continue;
                    }

                    // Take a look at the next token.
                    switch (tokens[t + 1].TokenType)
                    {
                        // This handles the case of `XYZ(`
                        case TokenType.OpenParenthesis:
                            continue;

                        // This handles the case of `XYZ:`
                        case TokenType.Colon:
                            int iIndentationLevel = 0;
                            int iEndTokenPos = t + 1;
                            Token iToken;
                            
                            while ((iToken = tokens[iEndTokenPos]).TokenType != TokenType.Comma ||
                                   iIndentationLevel > 0)
                            {
                                switch (iToken.TokenType)
                                {
                                    case TokenType.OpenParenthesis:
                                    case TokenType.OpenBracket:
                                        iIndentationLevel++;
                                        break;
                                    
                                    case TokenType.ClosingParenthesis:
                                    case TokenType.ClosingBracket:
                                        iIndentationLevel--;
                                        break;
                                }

                                iEndTokenPos++;
                                if (iEndTokenPos >= tokens.Length)
                                    break;
                            }

                            t += 2;
                            element ??= new ElementSet();
                            ((ElementSet) element).Elements.Add((string) token.Literal, ParseElement(tokens[t..iEndTokenPos]));
                            t = iEndTokenPos;

                            break;
                    }
                    
                    break;
                
                case TokenType.OpenParenthesis:
                    int pIndentationLevel = 0;
                    int pEndTokenPos = t;

                    Token pToken;
                    while ((pToken = tokens[++pEndTokenPos]).TokenType != TokenType.ClosingParenthesis ||
                           pIndentationLevel > 0)
                    {
                        switch (pToken.TokenType)
                        {
                            case TokenType.OpenParenthesis:
                            case TokenType.OpenBracket:
                                pIndentationLevel++;
                                break;
                            
                            case TokenType.ClosingParenthesis:
                            case TokenType.ClosingBrace:
                                pIndentationLevel--;
                                break;
                        }
                    }
                    
                    t++;
                    return ParseElement(tokens[t..pEndTokenPos]);
                    
                    break;

                case TokenType.String:
                    element = new ValueElement<string>((string) token.Literal, ElementType.String);
                    break;
                    
                case TokenType.Char:
                    element = new ValueElement<char>((char) token.Literal, ElementType.Char);
                    break;
                    
                case TokenType.Number:
                    element = new ValueElement<double>((double) token.Literal, ElementType.Number);
                    break;
                
                case TokenType.True:
                    element = new ValueElement<bool>(true, ElementType.Bool);
                    break;
                case TokenType.False:
                    element = new ValueElement<bool>(false, ElementType.Bool);
                    break;
                
                case TokenType.Eof:
                    break;

                //default:
                //    throw new Exception($"Unexpected token on line {token.Line}. (Token: {token.TokenType})");
            }
        }

        return element;
    }
}