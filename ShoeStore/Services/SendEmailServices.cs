using Microsoft.Extensions.Options;
using MimeKit;

namespace ShoeStore.Services
{
	public class SendEmail
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string DisplayName { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }


	}
	// Tao Interface cho email
	public interface ISendEmail
	{
		Task SendEmailAsync(string email, string subject, string Htmlmessage);
	}
	public class SendEmailServices : ISendEmail
	{
		private readonly SendEmail _sendEmail;

		public SendEmailServices(IOptions<SendEmail> options)
		{
			_sendEmail = options.Value;
		}

		public async Task SendEmailAsync(string email, string subject, string Htmlmessage)
		{
			var message = new MimeKit.MimeMessage();
			message.Sender = new MimeKit.MailboxAddress(_sendEmail.DisplayName, _sendEmail.Email);
			message.From.Add(new MailboxAddress(_sendEmail.DisplayName, _sendEmail.Email));
			message.Subject = subject;
			message.To.Add(MailboxAddress.Parse(email));

			var builder = new BodyBuilder();
			builder.HtmlBody = Htmlmessage;
			message.Body = builder.ToMessageBody();
			// dùng SmtpClient của MailKit
			using var smtp = new MailKit.Net.Smtp.SmtpClient();
			try
			{
				smtp.Connect(_sendEmail.Host, _sendEmail.Port, MailKit.Security.SecureSocketOptions.StartTls);
				smtp.Authenticate(_sendEmail.Email, _sendEmail.Password);
				await smtp.SendAsync(message);
			}

			catch (Exception ex)
			{
				// Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
				System.IO.Directory.CreateDirectory("mailssave");
				var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
				await message.WriteToAsync(emailsavefile);
			}

			smtp.Disconnect(true);
		}
	}
}
