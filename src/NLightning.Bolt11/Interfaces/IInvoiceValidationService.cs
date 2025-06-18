namespace NLightning.Bolt11.Interfaces;

using Models;

public interface IInvoiceValidationService
{
    ValidationResult ValidateInvoice(Invoice invoice);
    ValidationResult ValidateRequiredFields(Invoice invoice);
    ValidationResult ValidateFieldCombinations(Invoice invoice);
}