using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ogdi.Azure.QueueEntities; 

namespace InteractiveSdk.WorkerRole.MessageHandlers
{
	/// <summary>
	/// Sends an email using SMTP.
	/// </summary>
	internal class SendMailHandler : IMessageHandler
	{
		/// <summary>
		/// Sends an email.
		/// </summary>
		/// <param name="msg">XML formatted email information.</param>
		public void ProcessMessage(string msg)
		{
			if (msg == null) throw new ArgumentNullException("msg");

			using (var strReader = new StringReader(msg.Trim()))
			{
				var xmlSerializer = new XmlSerializer(typeof(EMailMessage));
				var mailMesssage = (EMailMessage)xmlSerializer.Deserialize(strReader);
				try
				{
					SendMessage(mailMesssage);
				}
				catch(SmtpFailedRecipientsException recipientsException)
				{
					// TODO Consider retry later.
					Trace.TraceInformation(recipientsException.Message);
					throw;
				}
				catch (SmtpException smtpException)
				{
					Trace.TraceInformation(smtpException.Message);
					throw;
				}
			}
		}

		private static void SendMessage(EMailMessage eMessage)
		{
			var smtpSettings = RoleEnvironment.GetConfigurationSettingValue("SmtpSettings");
			var parts = smtpSettings.Split(',', ';');
			if(parts.Length < 6)
			{
				Trace.TraceError("SmtpSettings must contain 6 parts: host, [port#], [<ssl>], from, [user],[password].");
				return;
			}

			int i = 0;
			var host = parts[i++].Trim();
			var port = parts[i++].Trim();
			var ssl = parts[i++].Trim();
			var from = parts[i++].Trim();
			var accountName = parts[i++].Trim();
			var password = parts[i];
			int portNumber;
			int.TryParse(port, out portNumber);

			var message = new MailMessage(from, eMessage.To, eMessage.Subject, eMessage.Body)
			{
				IsBodyHtml = eMessage.IsBodyHtml
			};

			var smtpClient = new SmtpClient(host)
			{
				EnableSsl = ssl.ToLowerInvariant() == "ssl",
				UseDefaultCredentials = false,
			};

			if (portNumber != 0)
			{
				smtpClient.Port = portNumber;
			}

			if (!string.IsNullOrEmpty(accountName))
			{
				smtpClient.Credentials = new NetworkCredential(accountName, password);
			}
			else
			{
				Trace.TraceError("Anonymous SMTP authentication will be applied.");
			}

			smtpClient.Send(message);
		}
	}
}
