﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jace.Tokenizer
{
    /// <summary>
    /// A token reader that converts the input string in a list of tokens.
    /// </summary>
    public class TokenReader
    {
        private readonly CultureInfo cultureInfo;
        private readonly char decimalSeparator; 

        public TokenReader() 
            : this(CultureInfo.CurrentCulture)
        {
        }

        public TokenReader(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
            this.decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator[0];
        }

        /// <summary>
        /// Read in the provided formula and convert it into a list of takens that can be processed by the
        /// Abstract Syntax Tree Builder.
        /// </summary>
        /// <param name="formula">The formula that must be converted into a list of tokens.</param>
        /// <returns>The list of tokens for the provided formula.</returns>
        public List<Token> Read(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                throw new ArgumentNullException("formula");

            List<Token> tokens = new List<Token>();

            char[] characters = formula.ToCharArray();

            bool isFormulaSubPart = true;

            for(int i = 0; i < characters.Length; i++)
            {
                if (IsPartOfNumeric(characters[i], true, isFormulaSubPart))
                {
                    string buffer = "" + characters[i];
                    int startPosition = i;

                    while (++i < characters.Length && IsPartOfNumeric(characters[i], false, isFormulaSubPart))
                    {
                        buffer += characters[i];
                    }

                    // Verify if we do not have an int
                    int intValue;
                    if (int.TryParse(buffer, out intValue))
                    {
                        tokens.Add(new Token() { TokenType = TokenType.Integer, Value = intValue, StartPosition = startPosition, Length = i - startPosition });
                        isFormulaSubPart = false;
                    }
                    else
                    {
                        double doubleValue;
                        if (double.TryParse(buffer, NumberStyles.Float | NumberStyles.AllowThousands,
                            cultureInfo, out doubleValue))
                        {
                            tokens.Add(new Token() { TokenType = TokenType.FloatingPoint, Value = doubleValue, StartPosition = startPosition, Length = i - startPosition });
                            isFormulaSubPart = false;
                        }
                        else if (buffer == "-")
                        {
                            // verify if we have a unary minus
                            tokens.Add(new Token() { TokenType = TokenType.Operation, Value = '-', StartPosition = startPosition, Length = 1 });
                        }
                        // Else we skip
                    }

                    if (i == characters.Length)
                    {
                        // Last character read
                        continue;
                    }
                }

                if (IsPartOfVariable(characters[i], true))
                {
                    string buffer = "" + characters[i];
                    int startPosition = i;

                    while (++i < characters.Length && IsPartOfVariable(characters[i], false))
                    {
                        buffer += characters[i];
                    }

                    tokens.Add(new Token() { TokenType = TokenType.Text, Value = buffer, StartPosition = startPosition, Length = i -startPosition });
                    isFormulaSubPart = false;

                    if (i == characters.Length)
                    {
                        // Last character read
                        continue;
                    }
                }

                switch (characters[i])
                { 
                    case ' ':
                        continue;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                    case '%':
                        tokens.Add(new Token() { TokenType = TokenType.Operation, Value = characters[i], StartPosition = i, Length = 1 });
                        isFormulaSubPart = true;
                        break;
                    case '(':
                        tokens.Add(new Token() { TokenType = TokenType.LeftBracket, Value = characters[i], StartPosition = i, Length = 1 });
                        isFormulaSubPart = true;
                        break;
                    case ')':
                        tokens.Add(new Token() { TokenType = TokenType.RightBracket, Value = characters[i], StartPosition = i, Length = 1 });
                        isFormulaSubPart = false;
                        break;
                    default:
                        break;
                }
            }

            return tokens;
        }

        private bool IsPartOfNumeric(char character, bool isFirstCharacter, bool isFormulaSubPart)
        {
            return character == decimalSeparator || (character >= '0' && character <= '9') || (isFormulaSubPart && isFirstCharacter && character == '-');
        }

        private bool IsPartOfVariable(char character, bool isFirstCharacter)
        {
            return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') || (!isFirstCharacter && character >= '0' && character <= '9');
        }
    }
}
