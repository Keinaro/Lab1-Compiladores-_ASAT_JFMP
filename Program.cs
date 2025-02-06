using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TokenType
{
    PLUS, MINUS, MULTIPLY, DIVIDE, LPAREN, RPAREN, SEMICOLON,
    BIN, OCT, HEX, ID, BINARY_NUMBER, OCTAL_NUMBER, HEXADECIMAL_NUMBER,
    EOF, INVALID
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class Lexer
{
    private string _input;
    private int _position;
    private static readonly Dictionary<TokenType, string> TokenPatterns = new() {
        { TokenType.PLUS, "\\+" },
        { TokenType.MINUS, "\\-" },
        { TokenType.MULTIPLY, "\\*" },
        { TokenType.DIVIDE, "/" },
        { TokenType.LPAREN, "\\(" },
        { TokenType.RPAREN, "\\)" },
        { TokenType.SEMICOLON, ";" },
        { TokenType.BIN, "\\b(bin)\\b" },
        { TokenType.OCT, "\\b(oct)\\b" },
        { TokenType.HEX, "\\b(hex)\\b" },
        { TokenType.ID, "\\b[a-zA-Z_][a-zA-Z0-9_]*\\b" },
        { TokenType.BINARY_NUMBER, "\\b[01]+\\b" },
        { TokenType.OCTAL_NUMBER, "\\b[0-7]+\\b" },
        { TokenType.HEXADECIMAL_NUMBER, "\\b[0-9A-F]+\\b" }
    };

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new();
        while (_position < _input.Length)
        {
            if (char.IsWhiteSpace(_input[_position]))
            {
                _position++;
                continue;
            }

            bool matched = false;
            foreach (var pattern in TokenPatterns)
            {
                var regex = new Regex("^" + pattern.Value, RegexOptions.IgnoreCase);
                var match = regex.Match(_input[_position..]);
                if (match.Success)
                {
                    tokens.Add(new Token(pattern.Key, match.Value));
                    _position += match.Length;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                tokens.Add(new Token(TokenType.INVALID, _input[_position].ToString()));
                _position++;
            }
        }
        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }
}

public class VariableTable
{
    private Dictionary<string, TokenType> variables = new();

    public bool DeclareVariable(string name, TokenType type, string value)
    {
        if (variables.ContainsKey(name)) return false;

        // Validamos que el valor coincida con la base
        bool isValid = type switch
        {
            TokenType.BIN => Regex.IsMatch(value, "^[01]+$"),
            TokenType.OCT => Regex.IsMatch(value, "^[0-7]+$"),
            TokenType.HEX => Regex.IsMatch(value, "^[0-9A-F]+$", RegexOptions.IgnoreCase),
            _ => false
        };

        if (!isValid)
        {
            Console.WriteLine($"Error: El valor '{value}' no es válido para el tipo {type}.");
            return false;
        }

        variables[name] = type;
        return true;
    }

    public bool IsValidVariable(string name) => variables.ContainsKey(name);
}

class Program
{
    static void Main()
    {
        VariableTable varTable = new();
        Console.WriteLine("Ingrese declaraciones de variables (ejemplo: bin var1 1010;), finalice con 'END'");

        while (true)
        {
            string declaration = Console.ReadLine();
            if (declaration == "END") break;

            Lexer lexer = new Lexer(declaration);
            List<Token> tokens = lexer.Tokenize();

            if (tokens.Count >= 5 &&
                (tokens[0].Type == TokenType.BIN || tokens[0].Type == TokenType.OCT || tokens[0].Type == TokenType.HEX) &&
                tokens[1].Type == TokenType.ID &&
                (tokens[2].Type == TokenType.BINARY_NUMBER || tokens[2].Type == TokenType.OCTAL_NUMBER || tokens[2].Type == TokenType.HEXADECIMAL_NUMBER) &&
                tokens[3].Type == TokenType.SEMICOLON)
            {
                string varName = tokens[1].Value;
                TokenType varType = tokens[0].Type;
                string varValue = tokens[2].Value;

                if (varTable.DeclareVariable(varName, varType, varValue))
                    Console.WriteLine($"Variable {varName} declarada correctamente.");
                else
                    Console.WriteLine($"Error: La variable {varName} ya fue declarada o tiene un valor incorrecto.");
            }
            else
            {
                Console.WriteLine("Error en la declaración de variables.");
            }
        }

        Console.WriteLine("Ingrese una expresión para analizar:");
        string expression = Console.ReadLine();
        Lexer exprLexer = new Lexer(expression);
        List<Token> exprTokens = exprLexer.Tokenize();

        foreach (var token in exprTokens)
        {
            if (token.Type == TokenType.ID && !varTable.IsValidVariable(token.Value))
            {
                Console.WriteLine($"Error: La variable {token.Value} no fue declarada.");
                return;
            }
        }

        Console.WriteLine("Expresión válida.");
    }
}
//Funcionando casi todo.
