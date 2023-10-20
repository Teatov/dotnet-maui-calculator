using System.Diagnostics;
using System.Globalization;

namespace Calculator.ViewModels;

[INotifyPropertyChanged]
internal partial class CalculatorPageViewModel
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string calculatedResult = "0";

    private int unclosedBrackets = 0;
    private bool usedDecimalPoint = false;
    private readonly string decimalSeparator = CultureInfo
        .CurrentCulture
        .NumberFormat
        .NumberDecimalSeparator;

    public CalculatorPageViewModel() { }

    [RelayCommand]
    private void Reset()
    {
        CalculatedResult = "0";
        InputText = "";
        unclosedBrackets = 0;
        usedDecimalPoint = false;
    }

    [RelayCommand]
    private void Calculate()
    {
        if (InputText.Length == 0)
            return;

        if (unclosedBrackets > 0)
        {
            InputText += new string(')', unclosedBrackets);
            unclosedBrackets = 0;
        }

        try
        {
            _ = MathsEvaluator.TryParse(InputText, out float result);

            CalculatedResult = result.ToString();
        }
        catch (DivideByZeroException)
        {
            CalculatedResult = "∞";
        }
        catch (Exception)
        {
            CalculatedResult = "ERROR";
        }
    }

    [RelayCommand]
    private void Backspace()
    {
        if (InputText.Length == 0)
            return;

        if (InputText.LastOrDefault() == '(')
            unclosedBrackets--;

        if (InputText.LastOrDefault() == ')')
            unclosedBrackets++;

        if (InputText.LastOrDefault() == decimalSeparator.Last())
            usedDecimalPoint = false;

        InputText = InputText[..^1];
    }

    [RelayCommand]
    private void NumberInput(string key)
    {
        if (
            !char.IsNumber(InputText.LastOrDefault())
            && InputText.LastOrDefault() != decimalSeparator.LastOrDefault()
        )
            usedDecimalPoint = false;

        InputText += key;
    }

    [RelayCommand]
    private void DecimalInput()
    {
        if (!char.IsNumber(InputText.LastOrDefault()))
            return;

        if (usedDecimalPoint)
            return;
        else
            usedDecimalPoint = true;

        InputText += decimalSeparator;
    }

    [RelayCommand]
    private void MathOperator(string op)
    {
        char[] operators = { '+', '-', '*', '/' };
        char prevChar = InputText.ElementAtOrDefault(InputText.Length - 2);
        if (
            operators.Contains(InputText.LastOrDefault())
            && (char.IsNumber(prevChar) || prevChar == ')')
        )
        {
            InputText = InputText[..^1] + op;
            return;
        }

        if (
            (
                op != "-"
                && (!char.IsNumber(InputText.LastOrDefault()) || InputText.LastOrDefault() == '(')
                && InputText.LastOrDefault() != ')'
            )
            || InputText.LastOrDefault() == '-'
        )
            return;

        InputText += op;
    }

    [RelayCommand]
    private void RegionOperator(string op)
    {
        if (op == "(")
            unclosedBrackets++;

        if (op == ")")
        {
            if (unclosedBrackets > 0)
                unclosedBrackets--;
            else
                return;
        }

        InputText += op;
    }
}
