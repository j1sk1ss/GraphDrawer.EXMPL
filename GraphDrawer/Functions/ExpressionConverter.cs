using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphDrawer.Functions {
    public static class ExpressionConverter {
        private static readonly string[] Operators = { "-", "+", "/", "*","^","sin","cos", "tg", "ctg", "="};
        private static readonly Func<double, double, double>[] Operations = {
            (a1, a2) => a1 - a2,
            (a1, a2) => a1 + a2,
            (a1, a2) => a1 / a2,
            (a1, a2) => a1 * a2,
            Math.Pow,
            (a1, a2) => a1 * Math.Sin(a2),
            (a1, a2) => a1 * Math.Cos(a2),
            (a1, a2) => a1 * Math.Tan(a2),
            (a1, a2) => a1 * (1 / Math.Tan(a2))
        };

        public static double ConvertString(string expression) {
            var tokens = GetTokens(expression);
            
            var operandStack  = new Stack<double>();
            var operatorStack = new Stack<string>();
            var tokenIndex    = 0;

            while (tokenIndex < tokens.Count) {
                var token = tokens[tokenIndex];
                switch (token) {
                    case "(": {
                        var subExpr = GetSubExpression(tokens, ref tokenIndex);
                        operandStack.Push(ConvertString(subExpr));
                        continue;
                    }
                    case ")":
                        throw new ArgumentException("Неправильная скобочная последовательность!");
                }

                if (Array.IndexOf(Operators, token) >= 0) {
                    while (operatorStack.Count > 0 && Array.IndexOf(Operators, token) < Array.IndexOf(Operators, operatorStack.Peek())) {
                        var op    = operatorStack.Pop();
                        var arg2 = operandStack.Pop();
                        var arg1 = operandStack.Pop();
                        operandStack.Push(Operations[Array.IndexOf(Operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else operandStack.Push(double.Parse(token));
                
            
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0) {
                var op    = operatorStack.Pop();
                var arg2 = operandStack.Pop();
                var arg1 = operandStack.Pop();
            
                operandStack.Push(Operations[Array.IndexOf(Operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }

        private static string GetSubExpression(IReadOnlyList<string> tokens, ref int index) {
            try {
                var subExpr      = new StringBuilder();
                var parentLevels = 1;
            
                index++;
                while (index < tokens.Count && parentLevels > 0) {
                    var token = tokens[index];
                    if (tokens[index] == "(") parentLevels += 1;
                
                    if (tokens[index++] == ")") parentLevels -= 1;
                
                    if (parentLevels > 0) subExpr.Append(token + " ");
                }
            
                if (parentLevels > 0) 
                    throw new ArgumentException("Неправильная скобочная последовательность!");
            
                return subExpr.ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e}");
                return "0";
            }
        }

        private static List<string> GetTokens(string expression) => expression.Split(" ", 
            StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}