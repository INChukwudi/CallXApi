using Azure.Communication.Email;
using Azure;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

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
        using var httpClient = new HttpClient();

        var url = "https://api.formbox.co/api/Organisations/sendSwiftMail";

        var payload = new
        {
            addresses = new[] { email },
            subject = "Your CallX OTP Code - CallX.com",
            content = $@"
                <p>Hello,</p>
                <p>Use this code to verify your email for <strong>CallX</strong>:</p>
                <h2 style='font-size:24px; font-weight:bold; color:#333;'>{otp}</h2>
                <p>This code will expire in 10 minutes.</p>
                <p>Thank you,<br/>The CallX Team</p>
            ",
            attachments = new object[] { }
        };

        var json = JsonSerializer.Serialize(payload);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, httpContent);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Email API failed: {response.StatusCode} - {responseBody}");
        }

        Console.WriteLine("OTP email sent successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending OTP email: {ex.Message}");
    }
}

    // public async Task SendOTPEmailAsync(string email, string otp)
    // {
    //     try
    //     {
    //         var emailContent = new EmailContent("Your CallX OTP Code - CallX.com")
    //         {
    //             PlainText = $"Use this code to verify your email: {otp}. " +
    //                         $"It will expire in 10 minutes.\nProvided by CallX.",

    //             Html = $@"
    //                 <p>Hello,</p>
    //                 <p>Use this code to verify your email for <strong>CallX</strong>:</p>
    //                 <h2 style='font-size:24px; font-weight:bold; color:#333;'>{otp}</h2>
    //                 <p>This code will expire in 10 minutes.</p>
    //                 <p>Thank you,<br/>The CallX Team</p>
    //             "
    //         };

    //         var emailRecipients = new EmailRecipients(new[]
    //         {
    //             new EmailAddress(email)
    //         });

    //         var emailMessage = new EmailMessage(_sender, emailRecipients, emailContent);

    //         // Send
    //         var result = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

    //         Console.WriteLine("Email sent successfully");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error sending OTP: {ex.Message}");
    //     }
    // }



        public async Task SendPasswordEmailAsync(string email, string otp)
    {
        try
        {
            var emailContent = new EmailContent("Your CallX Password - CallX.com")
            {
                PlainText = $"Use this passwword to sign-in to your account on the Call-X site. ",

                Html = $@"
                    <p>Hello,</p>
                    <p>Use this password to sign-in to your account for <strong>CallX</strong>:</p>
                    <h2 style='font-size:24px; font-weight:bold; color:#333;'>{otp}</h2>
                    <p>Thank you,<br/>The CallX Team</p>
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
