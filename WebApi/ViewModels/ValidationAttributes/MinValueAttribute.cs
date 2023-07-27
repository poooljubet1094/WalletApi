using System.ComponentModel.DataAnnotations;

namespace WalletApi.ViewModels.ValidationAttributes;

public class MinValueAttribute : ValidationAttribute
{
    private readonly decimal _minValue;

    public MinValueAttribute(double minValue)
    {
        _minValue = (decimal)minValue;
    }

    public override bool IsValid(object value)
    {
        return (decimal) value >= _minValue;
    }
}