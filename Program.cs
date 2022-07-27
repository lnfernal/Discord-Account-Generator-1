using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using _2CaptchaAPI;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


const string TwoCaptchaApiKey = "INPUT YOUR KEY HERE";  // Your 2Captcha API Key
//const string SmsActivateApiKey = "INPUT YOUR KEY HERE";  // Your SMS-Activate API Key
const string ProxyDomain = "INPUT YOUR PROXY DOMAIN HERE";  // Your Proxy Domain
const string ProxyPort = "INPUT YOUR PROXY PORT HERE";  // Your Proxy Port
const string ProxyUserName = "INPUT YOUR PROXY USERNAME HERE"; // Your Proxy userName
const string ProxyPassword = "INPUT YOUR PROXY PASSWORD HERE";  // Your Proxy password


HttpClientHandler clientHandler = new HttpClientHandler();
clientHandler.Proxy = new WebProxy()
{
    Address = new Uri($"http://{ProxyDomain}:{ProxyPort}"), 
    BypassProxyOnLocal = false,
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential(ProxyUserName, ProxyPassword) 
};


HttpClient Client = new HttpClient();
Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.115 Safari/537.36");
Client.DefaultRequestHeaders.Add("Host", "discord.com");
Client.DefaultRequestHeaders.Add("Origin", "https://discord.com");
Client.DefaultRequestHeaders.Add("Referer", "https://discord.com/channels/@me");
Client.DefaultRequestHeaders.Add("Connection", "keep-alive");

//HttpClient SMSClient = new HttpClient();

Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.115 Safari/537.36");
static async Task<string> GenFingerPrint(HttpClient Client)
{
    HttpResponseMessage response = await Client.PostAsync("https://discord.com/api/v9/auth/fingerprint",null);
    dynamic jsonresponse = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
    string fingerprint = jsonresponse["fingerprint"];
    return fingerprint;
}

static async Task<string> GenCaptchaKey()
{
    Console.WriteLine("Solving Main Captcha...");
    var captcha = new _2Captcha(TwoCaptchaApiKey);
    var captchasolution = await captcha.SolveHCaptcha("4c672d35-0701-42b2-88c3-78380b0db560", "https://discord.com/register");
    Console.WriteLine("Main Captcha Solved!");
    return captchasolution.Response;
}

static async Task<string> GenVerifyCaptchaKey()
{
    Console.WriteLine("Solving Email Captcha...");
    var captcha = new _2Captcha(TwoCaptchaApiKey);
    var captchasolution = await captcha.SolveHCaptcha("f5561ba9-8f1e-40ca-9b5b-a0b3f719ef34", "https://discord.com/verify");
    Console.WriteLine("Email Captcha Solved!");
    return captchasolution.Response;
}

/*static async Task<string> GenPhoneCaptchaKey()
{
    Console.WriteLine("Solving Phone Captcha");
    var captcha = new _2Captcha(TwoCaptchaAPIKey);
    var captchasolution = await captcha.SolveHCaptcha("f5561ba9-8f1e-40ca-9b5b-a0b3f719ef34", "https://discord.com/channels/@me");
    Console.WriteLine("Phone Captcha Solved");
    return captchasolution.Response;
}*/


static string GenBirth()
{
    Random rnd = new Random();
    int day = rnd.Next(1, 30);
    int month = rnd.Next(1, 13);
    int year = rnd.Next(1990, 2008);
    return $"{year}-{month}-{day}";
}


static string RandomString(int length)
{
    Random rnd = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[rnd.Next(s.Length)]).ToArray());
}

Console.Title = "Discord Account Generator | github.com/xDecider";
Console.WriteLine("Generating Details...");
string fingerprintz = GenFingerPrint(Client).Result;
Client.DefaultRequestHeaders.Add("x-fingerprint", fingerprintz);
string captchakey = GenCaptchaKey().Result;
string verifycaptchakey = GenVerifyCaptchaKey().Result;
//string phonecaptchakey = GenPhoneCaptchaKey().Result;
string birthday = GenBirth();
string usernamez = RandomString(10);
string passwordz = RandomString(15);
string emailtoken = Discord_Gen.EmailValidation.RandomEmail();
Console.WriteLine(emailtoken);
string inboxservice = Discord_Gen.EmailValidation.GetInboxService();

string payload = JsonConvert.SerializeObject(new
{
    captcha_key = captchakey,
    consent = true,
    date_of_birth = birthday,
    email = $"test-{emailtoken}{inboxservice}",
    fingerprint = fingerprintz,
    gift_code_sku_id = "null",
    invite = "null",
    password = passwordz,
    promotional_email_opt_in = false,
    username = usernamez
});
Console.WriteLine("");
Console.WriteLine("Creating Account...");
var content = new StringContent(payload, Encoding.UTF8, "application/json");
HttpResponseMessage response = await Client.PostAsync(new Uri("https://discord.com/api/v9/auth/register"), content);
if (response.IsSuccessStatusCode)
{
    dynamic jsonresponse = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
    string cookietoken = jsonresponse["token"];
    Client.DefaultRequestHeaders.Add("authorization", cookietoken);
    Console.WriteLine("Created Account!");
    await using (StreamWriter streamWriter = File.AppendText("generated.txt"))
    {
        streamWriter.WriteLine($"test-{emailtoken}{inboxservice}" + ":" + passwordz);
    }
    
    Console.WriteLine("Please wait...");
    Thread.Sleep(25000); //adjust this to your needs, Discord may be slow sometimes
    Console.WriteLine("");
    Console.WriteLine("Verifying Email...");
    string VerifyLink = await Discord_Gen.EmailValidation.VerifyEmail(emailtoken);
    if (VerifyLink != "") 
    {
        var request = (HttpWebRequest)HttpWebRequest.Create(VerifyLink);
        request.Method = "POST";
        request.AllowAutoRedirect = true;
        request.ContentType = "application/x-www-form-urlencoded";
        var redirectresponse = request.GetResponse();
        VerifyLink = redirectresponse.ResponseUri.ToString();
        Console.WriteLine(VerifyLink);
        string tokenz = VerifyLink.Replace("https://discord.com/verify#token=", "");
        string VerifyPayload = JsonConvert.SerializeObject(new
        {
            token = tokenz,
            captcha_key = verifycaptchakey
        });
        var verifycontent = new StringContent(VerifyPayload, Encoding.UTF8, "application/json");
        await Client.PostAsync(new Uri("https://discord.com/api/v9/auth/register"), verifycontent);
        Console.WriteLine("Email Verified Account!");

        
        
        /*Console.WriteLine("");
        Console.WriteLine("Verifying Phone...");
        string activationId = "";
        string phoneNumber = "";
        HttpResponseMessage phoneresponse = await SMSClient.GetAsync($"https://sms-activate.org/stubs/handler_api.php?api_key={SmsActivateApiKey}&action=getNumberV2&service=ds&country=16");
        dynamic smsendjsonresponse = JsonConvert.DeserializeObject(phoneresponse.Content.ReadAsStringAsync().Result);
        activationId = smsendjsonresponse["activationId"];
        phoneNumber = smsendjsonresponse["phoneNumber"];
        Console.WriteLine(phoneNumber);
        Console.WriteLine(activationId);
        string verifyphonepayload = JsonConvert.SerializeObject(new
        {
            change_phone_reason = "user_action_required",
            captcha_key = phonecaptchakey,
            phone = $"+{phoneNumber}"
        });
        var verifyphonecontent = new StringContent(verifyphonepayload, Encoding.UTF8, "application/json");
        await Client.PostAsync(new Uri("https://discord.com/api/v9/users/@me/phone"), verifyphonecontent);
        Console.WriteLine("Getting Code...");
        Thread.Sleep(5000);
        string codez = "";
        while (codez == "")
        {
            HttpResponseMessage coderesponse = await SMSClient.GetAsync($"https://api.sms-activate.org/stubs/handler_api.php?api_key={SmsActivateApiKey}&action=getStatus&id={activationId}");
            if (coderesponse.Content.ReadAsStringAsync().Result.Contains("STATUS_OK"))
            {
                codez = coderesponse.Content.ReadAsStringAsync().Result.Replace("STATUS_OK:", "");
            }
            else
            {
                Console.WriteLine("Waiting For Code...");
                Thread.Sleep(5000);
            }
        }
        Console.WriteLine(codez);

        string sendphonepayload = JsonConvert.SerializeObject(new
        {
            code = codez,
            phone = $"+{phoneNumber}"
        });
        var sendphonecontent = new StringContent(sendphonepayload, Encoding.UTF8, "application/json");
        HttpResponseMessage sendcoderesponse = await Client.PostAsync(new Uri("https://discord.com/api/v9/phone-verifications/verify"), sendphonecontent);
        dynamic jsonsendcoderesponse = JsonConvert.DeserializeObject(sendcoderesponse.Content.ReadAsStringAsync().Result);
        string codetoken = jsonsendcoderesponse["token"];
        Console.WriteLine(codetoken);

        string passwordandphonepayload = JsonConvert.SerializeObject(new
        {
            change_phone_reason = "user_action_required",
            password = password,
            phone_token = codetoken
        });
        var sendpasswordandphonecontent = new StringContent(passwordandphonepayload, Encoding.UTF8, "application/json");
        HttpResponseMessage sendcodetokenresponse = await Client.PostAsync(new Uri("https://discord.com/api/v9/phone-verifications/verify"), sendpasswordandphonecontent);
        Console.WriteLine(sendcodetokenresponse.Content.ReadAsStringAsync().Result);
        Console.WriteLine("Phone Verified!");*/
    }
    else
    {
        Console.Beep();
        Console.WriteLine("Failed to Get Verification Link!");
    }

}
else
{
    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
}