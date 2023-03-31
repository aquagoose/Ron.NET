using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ron.NET;

public static class Ron
{
    public static Element Parse(string ron)
    {
        RonParser parser = new RonParser(ron);

        Token[] tokens = parser.Parse();

        return ParseElement(tokens);
    }

    public static T Deserialize<T>(string ron)
    {
        Type type = typeof(T);
        foreach (ConstructorInfo info in type.GetConstructors())
        {
            if (info.GetParameters().Length == 0)
                goto SKIP_ERROR;
        }

        throw new InvalidOperationException("A parameterless constructor must be defined.");
        
        SKIP_ERROR:
        Element element = Parse(ron);

        T obj = Activator.CreateInstance<T>();

        for (int i = 0; i < element.NumSubElements; i++)
        {
            FieldInfo fInfo;
            
            //if ((fInfo = type.GetField()) != null)
        }

        return default;
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