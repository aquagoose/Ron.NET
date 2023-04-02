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

    public static string Serialize(object obj)
    {
        return _genMethods.Methods[obj.GetType()].serialize(obj).ToString();
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
                {
                    // Handle enums.
                    if (tokens.Length == 1)
                    {
                        // Enums are stored as strings as there is no other way to store them. RonGen is clever enough
                        // to recognize this and will convert to an enum.
                        element = new ValueElement<string>((string) token.Literal, ElementType.Enum);
                        continue;
                    }

                    int indentationLevels = 0;
                    int endToken = t + 1;

                    while (true)
                    {
                        switch (tokens[endToken++].TokenType)
                        {
                            case TokenType.OpenParenthesis:
                                indentationLevels++;
                                break;
                            
                            case TokenType.ClosingParenthesis:
                                indentationLevels--;

                                if (indentationLevels < 0)
                                    goto HANDLE_IDENTIFIER;

                                break;
                            
                            case TokenType.OpenBracket:
                                indentationLevels++;
                                break;
                            
                            case TokenType.ClosingBracket:
                                indentationLevels--;
                                break;
                            
                            case TokenType.Eof:
                                goto HANDLE_IDENTIFIER;
                                
                            case TokenType.Comma when indentationLevels == 0:
                                goto HANDLE_IDENTIFIER;
                        }
                    }
                    
                    HANDLE_IDENTIFIER:
                    endToken--;
                    element ??= new ElementSet();
                    ((ElementSet) element).Elements.Add((string) token.Literal, ParseElement(tokens[(t + 2)..endToken]));
                    t = endToken;

                    break;
                }

                case TokenType.OpenBracket:
                    Token lastToken = tokens[t];
                    int tokenIndex = t;
                    int bIndentationLevels = 0;
                    
                    while (true)
                    {
                        ref Token nextToken = ref tokens[++tokenIndex];

                        switch (nextToken.TokenType)
                        {
                            case TokenType.OpenParenthesis:
                                bIndentationLevels++;
                                break;
                            
                            case TokenType.ClosingParenthesis:
                                bIndentationLevels--;
                                break;
                            
                            case TokenType.OpenBracket:
                                bIndentationLevels++;
                                break;
                            
                            case TokenType.ClosingBracket:
                                bIndentationLevels--;

                                if (bIndentationLevels <= 0)
                                {
                                    if (lastToken.TokenType != TokenType.Comma)
                                    {
                                        element ??= new ElementArray();
                                        ((ElementArray) element).Elements.Add(ParseElement(tokens[(t + 1)..tokenIndex]));
                                        t = tokenIndex;
                                    }
                                }

                                goto BRACKET_EXIT;
                                
                            case TokenType.Comma when bIndentationLevels == 0:
                                element ??= new ElementArray();
                                Console.WriteLine(tokens[tokenIndex]);
                                ((ElementArray) element).Elements.Add(ParseElement(tokens[(t + 1)..tokenIndex]));
                                t = tokenIndex;
                                break;
                        }

                        lastToken = nextToken;
                    }

                    BRACKET_EXIT: ;
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

        EXIT: ;

        return element;
    }
}