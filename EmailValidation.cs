using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord_Gen
{
    public static class EmailValidation
    {

		public static string GetInboxService()
		{
			return "@test.mailgenius.com";
		}

		public static string RandomEmail()
		{
			string text = "0123456789abcdefghijklmnopqrstuvwxyz";
			char[] array = new char[6];
			Random random = new Random();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = text[random.Next(text.Length)];
			}
			return new string(array);
		}

		public static async Task<string> VerifyEmail(string emailtoken)
		{
			HttpClient Client = new HttpClient()
			{
				DefaultRequestHeaders = {{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36 OPR/85.0.4341.72 (Edition std-1)" }}
			};
			
			string DiscordURL = "https://click.discord.com/ls/click?upn=";
			HttpResponseMessage response = await Client.GetAsync("https://app.mailgenius.com/spam-test/" + emailtoken);
			string email = "";
			
			if (response.StatusCode == System.Net.HttpStatusCode.OK) 
			{
				string ResponseMsg = response.Content.ReadAsStringAsync().Result;
				email = DiscordURL + ResponseMsg.Split(DiscordURL)[2].Split("quot;")[0].Replace(@"\&", "");
			}
			return email;
		}
	}
}
