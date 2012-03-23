using System.IO;
using System.Net;
using System.Text;
using Ogdi.Azure.Configuration;

namespace Ogdi.InteractiveSdk.Mvc
{
	public static class Recaptcha
	{
		public static bool Validate(string challenge, string response, string userHostAddress)
		{
			StringBuilder postData = new StringBuilder();
			postData.AppendFormat("{0}={1}", "privatekey", OgdiConfiguration.GetValue("RecaptchaPrivateKey"));
			postData.AppendFormat("&{0}={1}", "remoteip", userHostAddress);
			postData.AppendFormat("&{0}={1}", "challenge", challenge);
			postData.AppendFormat("&{0}={1}", "response", response);

			HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://api-verify.recaptcha.net/verify");
			myReq.Method = WebRequestMethods.Http.Post;
			myReq.ContentType = "application/x-www-form-urlencoded";
			myReq.ContentLength = postData.Length;

			using (StreamWriter postwriter = new StreamWriter(myReq.GetRequestStream()))
			{
				postwriter.Write(postData);
			}

			string recaptchaResponse;
			HttpWebResponse myResp = myReq.GetResponse() as HttpWebResponse;
			using (StreamReader reader = new StreamReader(myResp.GetResponseStream()))
			{
				recaptchaResponse = reader.ReadLine();
				return recaptchaResponse.ToLower() == "true";
			}
		}
	}
}
