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
    
    public static Element Parse(string ron)
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

    private static Element ParseElement(Token[] tokens)
    {
        Element element = new Element();
        
        for (int t = 0; t < tokens.Length; t++)
        {
            ref Token token = ref tokens[t];

            switch (token.TokenType)
            {
                case TokenType.Identifier:
                {
                    if (tokens.Length == 1)
                    {
                        element = new Element((string) token.Literal);
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
                            
                            case TokenType.Eof:
                                goto HANDLE_IDENTIFIER;
                                
                            case TokenType.Comma when indentationLevels == 0:
                                goto HANDLE_IDENTIFIER;
                        }
                    }
                    
                    HANDLE_IDENTIFIER:
                    endToken--;
                    element.Elements.Add((string) token.Literal, ParseElement(tokens[(t + 2)..endToken]));
                    t = endToken;

                    break;
                }
                
                case TokenType.OpenBrace:
                    Console.WriteLine("MEP");
                    break;
                
                case TokenType.ClosingBrace:
                    Console.WriteLine("PEM");
                    break;
                
                case TokenType.String:
                case TokenType.Char:
                case TokenType.Number:
                    element = new Element(token.Literal);
                    break;
                
                case TokenType.True:
                    element = new Element(true);
                    break;
                case TokenType.False:
                    element = new Element(false);
                    break;
            }
        }

        return element;
    }
}