namespace NLightning.Bolt11.Services;

using Interfaces;
using Models;

public class InvoiceValidationService : IInvoiceValidationService
{
    public ValidationResult ValidateInvoice(Invoice invoice)
    {
        var results = new List<ValidationResult>
        {
            ValidateRequiredFields(invoice),
            ValidateFieldCombinations(invoice),
        };

        var errors = results.SelectMany(r => r.Errors).ToList();
        return new ValidationResult(errors.Count == 0, errors);
    }

    public ValidationResult ValidateRequiredFields(Invoice invoice)
    {
        var errors = new List<string>();

        // Payment hash is required (p field)
        if (invoice.PaymentHash is null)
            errors.Add($"{nameof(invoice.PaymentHash)} is required");

        // Payment secret is required (s field)
        if (invoice.PaymentSecret is null)
            errors.Add($"{nameof(invoice.PaymentSecret)} is required");

        // Either description or description hash is required (d or h field)
        var hasDescription = invoice.Description is not null;
        var hasDescriptionHash = invoice.DescriptionHash is not null;
        if (!hasDescription && !hasDescriptionHash)
            errors.Add($"Either {nameof(invoice.Description)} or {nameof(invoice.DescriptionHash)} is required");

        return new ValidationResult(errors.Count == 0, errors);
    }

    public ValidationResult ValidateFieldCombinations(Invoice invoice)
    {
        var errors = new List<string>();

        // Description and description hash are mutually exclusive
        var hasDescription = invoice.Description is not null;
        var hasDescriptionHash = invoice.DescriptionHash is not null;
        if (hasDescription && hasDescriptionHash)
            errors.Add($"{nameof(invoice.Description)} and {nameof(invoice.DescriptionHash)} cannot both be present");

        return new ValidationResult(errors.Count == 0, errors);
    }
}