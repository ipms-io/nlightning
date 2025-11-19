using System.Reflection;
using NBitcoin;

namespace NLightning.Bolt11.Tests.Services;

using Bolt11.Models;
using Bolt11.Models.TaggedFields;
using Bolt11.Services;
using Domain.Protocol.ValueObjects;
using Interfaces;

public class InvoiceValidationServiceTests
{
    private readonly InvoiceValidationService _validator;

    private readonly uint256 _invoiceHash =
        uint256.Parse("ed06856213fbdf7a60d0e679f0b8502125468ae268dc353475d019762aaa2c41");

    private readonly uint256 _invoiceSecret =
        uint256.Parse("e39ea727045763c32f60a262e3b2ec358b29183697edcaf4689e6b8a49df1cdf");

    private readonly uint256 _invoiceDescriptionHash =
        uint256.Parse("b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1ca");

    private const string InvoiceDescription = "Test invoice description";

    public InvoiceValidationServiceTests()
    {
        _validator = new InvoiceValidationService();
    }

    #region ValidateRequiredFields Tests

    [Fact]
    public void Given_InvoiceWithAllRequiredFields_When_ValidateRequiredFields_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_InvoiceMissingPaymentHash_When_ValidateRequiredFields_Then_ResultIsInvalidWithCorrectError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"{nameof(invoice.PaymentHash)} is required", result.Errors);
    }

    [Fact]
    public void Given_InvoiceMissingPaymentSecret_When_ValidateRequiredFields_Then_ResultIsInvalidWithCorrectError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"{nameof(invoice.PaymentSecret)} is required", result.Errors);
    }

    [Fact]
    public void
        Given_InvoiceMissingDescriptionAndDescriptionHash_When_ValidateRequiredFields_Then_ResultIsInvalidWithCorrectError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"Either {nameof(invoice.Description)} or {nameof(invoice.DescriptionHash)} is required",
                        result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithOnlyDescription_When_ValidateRequiredFields_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithOnlyDescriptionHash_When_ValidateRequiredFields_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            DescriptionHash = _invoiceDescriptionHash
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithEmptyDescription_When_ValidateRequiredFields_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            Description = string.Empty
        };

        // When
        var result = _validator.ValidateRequiredFields(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region ValidateFieldCombinations Tests

    [Fact]
    public void
        Given_InvoiceWithBothDescriptionAndDescriptionHash_When_ValidateFieldCombinations_Then_ResultIsInvalidWithCorrectError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            Description = InvoiceDescription
        };
        // Bypass the public setter for DescriptionHash due to checks
        var taggedFieldListProp =
            typeof(Invoice).GetField("_taggedFields", BindingFlags.NonPublic | BindingFlags.Instance)
         ?? throw new NullReferenceException("_taggedFields property not found");
        var taggedFields = taggedFieldListProp.GetValue(invoice) as List<ITaggedField>
                        ?? throw new NullReferenceException("TaggedFieldList not found");
        taggedFields.Add(new DescriptionHashTaggedField(_invoiceDescriptionHash));

        // When
        var result = _validator.ValidateFieldCombinations(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"{nameof(invoice.Description)} and {nameof(invoice.DescriptionHash)} cannot both be present",
                        result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithOnlyDescription_When_ValidateFieldCombinations_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            Description = InvoiceDescription
            // DescriptionHash = null
        };

        // When
        var result = _validator.ValidateFieldCombinations(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithOnlyDescriptionHash_When_ValidateFieldCombinations_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            // Description = null,
            DescriptionHash = _invoiceDescriptionHash
        };

        // When
        var result = _validator.ValidateFieldCombinations(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void
        Given_InvoiceWithNeitherDescriptionNorDescriptionHash_When_ValidateFieldCombinations_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            // Description = null,
            // DescriptionHash = null
        };

        // When
        var result = _validator.ValidateFieldCombinations(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region ValidateInvoice Tests

    [Fact]
    public void Given_ValidInvoice_When_ValidateInvoice_Then_ResultIsValid()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateInvoice(invoice);

        // Then
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithMissingRequiredField_When_ValidateInvoice_Then_ResultIsInvalidWithRequiredFieldError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            // PaymentHash = null, // Missing
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription
        };

        // When
        var result = _validator.ValidateInvoice(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"{nameof(invoice.PaymentHash)} is required", result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithInvalidFieldCombination_When_ValidateInvoice_Then_ResultIsInvalidWithCombinationError()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            PaymentHash = _invoiceHash,
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription,
        };
        // Bypass the public setter for DescriptionHash due to checks
        var taggedFieldListProp =
            typeof(Invoice).GetField("_taggedFields", BindingFlags.NonPublic | BindingFlags.Instance)
         ?? throw new NullReferenceException("_taggedFields property not found");
        var taggedFields = taggedFieldListProp.GetValue(invoice) as List<ITaggedField>
                        ?? throw new NullReferenceException("TaggedFieldList not found");
        taggedFields.Add(new DescriptionHashTaggedField(_invoiceDescriptionHash));

        // When
        var result = _validator.ValidateInvoice(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains($"{nameof(invoice.Description)} and {nameof(invoice.DescriptionHash)} cannot both be present",
                        result.Errors);
    }

    [Fact]
    public void Given_InvoiceWithMultipleErrors_When_ValidateInvoice_Then_ResultIsInvalidWithAllErrors()
    {
        // Given
        var invoice = new Invoice(BitcoinNetwork.Mainnet)
        {
            // PaymentHash = null, // Missing
            PaymentSecret = _invoiceSecret,
            Description = InvoiceDescription,
        };
        // Bypass the public setter for DescriptionHash due to checks
        var taggedFieldListProp =
            typeof(Invoice).GetField("_taggedFields", BindingFlags.NonPublic | BindingFlags.Instance)
         ?? throw new NullReferenceException("_taggedFields property not found");
        var taggedFields = taggedFieldListProp.GetValue(invoice) as List<ITaggedField>
                        ?? throw new NullReferenceException("TaggedFieldList not found");
        taggedFields.Add(new DescriptionHashTaggedField(_invoiceDescriptionHash));

        // When
        var result = _validator.ValidateInvoice(invoice);

        // Then
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains($"{nameof(invoice.PaymentHash)} is required", result.Errors);
        Assert.Contains($"{nameof(invoice.Description)} and {nameof(invoice.DescriptionHash)} cannot both be present",
                        result.Errors);
    }

    #endregion
}