using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Calculator.Models;

public static class MathsEvaluator
{
    public static float Parse(string expression)
    {
        if (float.TryParse(expression, out float d))
        {
            // выражение - просто число, поэтому просто возвращаем его
            Debug.WriteLine($"'{expression}' = '{d}'");
            return d;
        }
        else
        {
            return CalculateValue(expression);
        }
    }

    public static bool TryParse(string expression, out float value)
    {
        if (IsExpression(expression))
        {
            value = Parse(expression);
            return true;
        }
        else
        {
            Debug.WriteLine("TryParse");
            throw new SyntaxErrorException("Некорректное выражение");
        }
    }

    public static bool IsExpression(string s)
    {
        Regex RgxUrl = new("^[0-9+*-/()., ]+$");
        return RgxUrl.IsMatch(s);
    }

    private static List<string> TokenizeExpression(
        string expression,
        Dictionary<char, int> operators
    )
    {
        List<string> elements = new();
        string currentElement = string.Empty;

        BracketState state = BracketState.Start;
        int bracketCount = 0;

        for (int i = 0; i < expression.Length; i++)
        {
            switch (state)
            {
                case BracketState.Start:
                    if (expression[i] == '(')
                    {
                        state = BracketState.BracketOpened;
                        bracketCount = 0;
                        if (currentElement != string.Empty)
                        {
                            // если currentElement не пустой, то предполагается умножение, напр 5(1+2)
                            elements.Add(currentElement);
                            elements.Add("*");
                            currentElement = string.Empty;
                        }
                    }
                    else if (operators.ContainsKey(expression[i]))
                    {
                        elements.Add(currentElement);
                        elements.Add(expression[i].ToString());
                        currentElement = string.Empty;
                    }
                    else if (expression[i] != ' ')
                    {
                        currentElement += expression[i];
                    }
                    break;

                case BracketState.BracketOpened:
                    if (expression[i] == '(')
                    {
                        bracketCount++;
                        currentElement += expression[i];
                    }
                    else if (expression[i] == ')')
                    {
                        if (bracketCount == 0)
                        {
                            state = BracketState.BracketClosed;
                        }
                        else
                        {
                            bracketCount--;
                            currentElement += expression[i];
                        }
                    }
                    else if (expression[i] != ' ')
                    {
                        currentElement += expression[i];
                    }
                    break;

                case BracketState.BracketClosed:
                    if (operators.ContainsKey(expression[i]))
                    {
                        state = BracketState.Start;
                        elements.Add(currentElement);
                        currentElement = string.Empty;
                        elements.Add(expression[i].ToString());
                    }
                    else if (expression[i] != ' ')
                    {
                        elements.Add(currentElement);
                        elements.Add("*");
                        currentElement = string.Empty;

                        if (expression[i] == '(')
                        {
                            state = BracketState.BracketOpened;
                            bracketCount = 0;
                        }
                        else
                        {
                            currentElement += expression[i];
                            state = BracketState.Start;
                        }
                    }
                    break;
            }
        }

        // добавляем последний оставшийся элемент в список
        if (currentElement.Length > 0)
            elements.Add(currentElement);

        return elements;
    }

    private static float CalculateValue(string expression)
    {
        // операторы и их приоритет
        Dictionary<char, int> operators =
            new()
            {
                { '+', 1 },
                { '-', 1 },
                { '*', 2 },
                { '/', 2 }
            };

        List<string> elements = TokenizeExpression(expression, operators);
        Debug.WriteLine(string.Join(", ", elements.ToArray()));

        float? value = null;

        // идём с наибольшего приоритета к наименьшему
        for (int i = operators.Values.Max(); i >= operators.Values.Min(); i--)
        {
            // бегаем пока в списке есть операторы с текущим приоритетом
            while (
                elements.Count >= 3
                && elements.Any(
                    element =>
                        element.Length == 1
                        && operators
                            .Where(op => op.Value == i)
                            .Select(op => op.Key)
                            .Contains(element[0])
                )
            )
            {
                // позиция этого элемента
                int pos = elements.FindIndex(
                    element =>
                        element.Length == 1
                        && operators
                            .Where(op => op.Value == i)
                            .Select(op => op.Key)
                            .Contains(element[0])
                );

                Debug.WriteLine("CalculateValue in while " + value);
                value =
                    EvaluateOperation(elements[pos], elements[pos - 1], elements[pos + 1])
                    ?? throw new SyntaxErrorException("Некорректное выражение");
                // назначаем на место первого операнда вычисленное значение
                elements[pos - 1] = value.ToString();
                // удаляем оператор и второй операнд
                elements.RemoveRange(pos, 2);
            }
        }

        Debug.WriteLine("CalculateValue in return " + value);
        return value ?? throw new SyntaxErrorException("Некорректное выражение");
    }

    private static float? EvaluateOperation(string operation, string operand1, string operand2)
    {
        string[] operators = { "+", "-", "*", "/" };
        if (!operators.Contains(operation))
            throw new ArgumentException("Неподдерживаемый оператор");

        float op1 = Parse(operand1);
        float op2 = Parse(operand2);
        Debug.WriteLine($"'{op1}' {operation} '{op2}'");

        float? value = null;
        switch (operation)
        {
            case "+":
                value = op1 + op2;
                break;
            case "-":
                value = op1 - op2;
                break;
            case "*":
                value = op1 * op2;
                break;
            case "/":
                value = op1 / op2;
                break;
        }

        return value;
    }

    enum BracketState
    {
        Start,
        BracketOpened,
        BracketClosed
    }
}
