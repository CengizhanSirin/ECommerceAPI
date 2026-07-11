namespace ECommerce.Application.DTOs.Payment.Requests;

public record ProcessPaymentRequest(string CardHolderName, string CardNumber, string ExpirationMonth, string ExpirationYear, string Cvv);