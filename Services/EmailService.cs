using Azure.Communication.Email;
using Azure;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _sender;
    private string connectionString;
    private string senderAddress;

    public EmailService(IConfiguration config)
    {
        connectionString = config["AZURE_COMMUNICATION_CONNECTION"];
        senderAddress = config["ACS_SENDER"];
        _emailClient = new EmailClient(connectionString);
        _sender = senderAddress;
    }

    public async Task SendOTPEmailAsync(string email, string otp)
    {
        try
        {
            var emailContent = new EmailContent("Your CallX OTP Code - CallX.com")
            {
                PlainText = $"Use this code to verify your email: {otp}. " +
                            $"It will expire in 10 minutes.\nProvided by CallX.",

                Html = $@"
                    <p>Hello,</p>
                    <p>Use this code to verify your email for <strong>CallX</strong>:</p>
                    <h2 style='font-size:24px; font-weight:bold; color:#333;'>{otp}</h2>
                    <p>This code will expire in 10 minutes.</p>
                    <p>Thank you,<br/>The Pansive Team</p>
                "
            };

            var emailRecipients = new EmailRecipients(new[]
            {
                new EmailAddress(email)
            });

            var emailMessage = new EmailMessage(_sender, emailRecipients, emailContent);

            // Send
            var result = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            Console.WriteLine("Email sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending OTP: {ex.Message}");
        }
    }
}
