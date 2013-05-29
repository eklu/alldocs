using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Live;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace All_Docs
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    /// 
    public sealed partial class AddAccountPage : All_Docs.Common.LayoutAwarePage
    {
        private const string GoogleClientID = "76180523805-6oqf3foquodgi4vbviph2n2getb4mh1g.apps.googleusercontent.com";
        private const string GoogleCallbackUrl = "urn:ietf:wg:oauth:2.0:oob";
        private const string GoogleClientSecret = "Gr8zMdjPIqcHbfJ9-2lLWQVO";

        private const string SkyDriveClientID =
            "ms-app://s-1-15-2-3579037172-1036449843-2323837178-3479193454-884235038-515082261-2619872524";

        private const string SkyDriveClientSecret = "Q5FkQGM8sZvCmnNjgnTeHxum9uHyAlcA";
        private const string CallbackUrl = "http://www.alldocs.com/callback";
        private const string BoxCallbackUrl = "https://www.alldocs.com/callback";

        private const string BoxClientID = "o0uo9bzy9s59g3krse7drq5wyugm5hu0";
        private const string BoxClientSecret = "42fymsuzsuj3JBTfYjHE3QU9RLMlZyrk";


        private const string DropboxAppKey = "o3hgkbl252j90ci";
        private const string DropboxAppSecret = "jgz3mc4gwxspibg";

        ApplicationDataContainer roamingSettings = null;
        public AddAccountPage()
        {
            this.InitializeComponent();
            roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }


        private async void Add_SkyDrive(object sender, RoutedEventArgs e)
        {
            string msg, title;
            try
            {
                //get code
                String SkyDriveURL = "https://login.live.com/oauth20_authorize.srf?client_id=" + Uri.EscapeDataString(SkyDriveClientID) + "&redirect_uri=" + Uri.EscapeDataString(CallbackUrl) + "&response_type=code&scope=" + Uri.EscapeDataString("wl.basic wl.offline_access wl.emails wl.skydrive_update");
                

                System.Uri StartUri = new Uri(SkyDriveURL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                System.Uri EndUri = new Uri(CallbackUrl);


                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None,
                                                        StartUri,
                                                       EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                                 //if authorized, get access token
                    string Code =
                        WebAuthenticationResult.ResponseData.Substring(WebAuthenticationResult.ResponseData.IndexOf("=")+1);

                    HttpClient client = new HttpClient();
                    
                    // client.DefaultRequestHeaders.Authorization =AuthenticationHeaderValue.Parse("OAuth" + data);
                    
                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    postData.Add(new KeyValuePair<string, string>("client_secret", SkyDriveClientSecret));
                    postData.Add(new KeyValuePair<string, string>("client_id", SkyDriveClientID));
                    postData.Add(new KeyValuePair<string, string>("redirect_uri", CallbackUrl));
                    postData.Add(new KeyValuePair<string, string>("code", Code));
                    HttpContent content = new FormUrlEncodedContent(postData);

                    HttpResponseMessage res = await client.PostAsync("https://login.live.com/oauth20_token.srf", content);

                    if (res.IsSuccessStatusCode)
                    {
                        Stream stream = await res.Content.ReadAsStreamAsync();
                        StreamReader ResponseDataStream = new StreamReader(stream);

                        string jsonText = ResponseDataStream.ReadToEnd();

                        JsonObject jObject = JsonObject.Parse(jsonText);
                        
                        string AccessToken = ""+jObject["access_token"].GetString();
                        string RefreshToken = ""+jObject["refresh_token"].GetString();
                        string TokenType = ""+jObject["token_type"].GetString();
                        string ExpiresIn = ""+ jObject["expires_in"].GetNumber();
                        string AuthenticationToken = ""+jObject["authentication_token"].GetString();

                        HttpResponseMessage res1 =
                            await client.GetAsync("https://apis.live.net/v5.0/me?access_token=" + AccessToken);
                        if (res1.IsSuccessStatusCode)
                        {
                            Stream stream1 = await res1.Content.ReadAsStreamAsync();
                            StreamReader ResponseDataStream1 = new StreamReader(stream1);

                            string jsonText1 = ResponseDataStream1.ReadToEnd();

                            JsonObject jObject1 = JsonObject.Parse(jsonText1);
                            string Email = "" +  ((JsonObject) jObject1["emails"].GetObject())["account"].GetString();

                            roamingSettings.CreateContainer("Accounts", ApplicationDataCreateDisposition.Always);

                            if (roamingSettings.Containers.ContainsKey("Accounts"))
                            {
                                var composite = new ApplicationDataCompositeValue();
                                string key = "0" + Email;
                                composite["Type"] = "0";
                                composite["Login"] = Email;
                                composite["RefreshToken"] = RefreshToken;
                                composite["AccessToken"] = AccessToken;
                                composite["ExpiresIn"] = ExpiresIn;
                                composite["TokenType"] = TokenType;
                                composite["AuthenticationToken"] = AuthenticationToken;

                                roamingSettings.Containers["Accounts"].Values[key] = composite;

                            }
                        }
                    }
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseErrorDetail.ToString();

                }
                else
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseStatus.ToString();
                }

            }
            catch (Exception Error)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                title = "Error";
                msg = Error.ToString();
            }

            //var messageDialog = new MessageDialog(msg, title);

            // await messageDialog.ShowAsync();

            if (roamingSettings.Containers.ContainsKey("Accounts") &&
                roamingSettings.Containers["Accounts"].Values.Count > 0)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }
    

        async private void Add_Google(object sender, RoutedEventArgs e)
        {
            string msg, title;
            try
            {
                //get code
                String GoogleURL = "https://accounts.google.com/o/oauth2/auth?client_id=" + Uri.EscapeDataString(GoogleClientID) + "&redirect_uri=" + Uri.EscapeDataString(GoogleCallbackUrl) + "&response_type=code&approval_prompt=force&access_type=offline&scope=" + Uri.EscapeDataString("https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/drive");
                

                System.Uri StartUri = new Uri(GoogleURL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                System.Uri EndUri = new Uri("https://accounts.google.com/o/oauth2/approval?");


                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.UseTitle,
                                                        StartUri,
                                                        EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    
                    //if authorized, get access token
                    string Code =
                        WebAuthenticationResult.ResponseData.Substring(WebAuthenticationResult.ResponseData.IndexOf("=")+1);

                    HttpClient client = new HttpClient();
                    
                    // client.DefaultRequestHeaders.Authorization =AuthenticationHeaderValue.Parse("OAuth" + data);
                    
                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    postData.Add(new KeyValuePair<string, string>("client_secret", GoogleClientSecret));
                    postData.Add(new KeyValuePair<string, string>("client_id", GoogleClientID));
                    postData.Add(new KeyValuePair<string, string>("redirect_uri", GoogleCallbackUrl));
                    postData.Add(new KeyValuePair<string, string>("code", Code));
                    HttpContent content = new FormUrlEncodedContent(postData);

                    HttpResponseMessage res = await client.PostAsync("https://accounts.google.com/o/oauth2/token", content);

                    if (res.IsSuccessStatusCode)
                    {
                        Stream stream = await res.Content.ReadAsStreamAsync();
                        StreamReader ResponseDataStream = new StreamReader(stream);

                        string jsonText = ResponseDataStream.ReadToEnd();

                        JsonObject jObject = JsonObject.Parse(jsonText);
                        
                        string AccessToken = ""+jObject["access_token"].GetString();
                        string RefreshToken = ""+jObject["refresh_token"].GetString();
                        string TokenType = ""+jObject["token_type"].GetString();
                        string ExpiresIn = ""+ jObject["expires_in"].GetNumber();

                        HttpResponseMessage res1 =
                            await client.GetAsync("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + AccessToken);
                        if (res1.IsSuccessStatusCode)
                        {
                            Stream stream1 = await res1.Content.ReadAsStreamAsync();
                            StreamReader ResponseDataStream1 = new StreamReader(stream1);

                            string jsonText1 = ResponseDataStream1.ReadToEnd();

                            JsonObject jObject1 = JsonObject.Parse(jsonText1);
                            string Email = "" + jObject1["email"].GetString();

                            roamingSettings.CreateContainer("Accounts", ApplicationDataCreateDisposition.Always);

                            if (roamingSettings.Containers.ContainsKey("Accounts"))
                            {
                                var composite = new ApplicationDataCompositeValue();
                                string key = "1" + Email;
                                composite["Type"] = "1";
                                composite["Login"] = Email;
                                composite["RefreshToken"] = RefreshToken;
                                composite["AccessToken"] = AccessToken;
                                composite["ExpiresIn"] = ExpiresIn;
                                composite["TokenType"] = TokenType;

                                roamingSettings.Containers["Accounts"].Values[key] = composite;

                            }


                        }

                    }

                    //call user api, store in roaming settings
                    //todo store credentials
                    //store in roaming settings
                    msg = "Account added successfully.";
                    title = "Success!";


                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseErrorDetail.ToString();

                }
                else
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseStatus.ToString();
                }

            }
            catch (Exception Error)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                title = "Error";
                msg = Error.ToString();
            }

            //var messageDialog = new MessageDialog(msg, title);

            // await messageDialog.ShowAsync();

            if (roamingSettings.Containers.ContainsKey("Accounts") &&
                roamingSettings.Containers["Accounts"].Values.Count > 0)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        async private void Add_Dropbox(object sender, RoutedEventArgs e)
        {
            string msg, title;
            try
            {
                // 
                // Acquiring a request token 
                // 
                TimeSpan SinceEpoch = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                Random Rand = new Random();
                String DropboxUrl = "https://api.dropbox.com/1/oauth/request_token";
                Int32 Nonce = Rand.Next(1000000000);
                // 
                // Compute base signature string and sign it. 
                //    This is a common operation that is required for all requests even after the token is obtained. 
                //    Parameters need to be sorted in alphabetical order 
                //    Keys and values should be URL Encoded. 
                // 
                String SigBaseStringParams = "oauth_callback=" + Uri.EscapeDataString(CallbackUrl);
                SigBaseStringParams += "&" + "oauth_consumer_key=" + DropboxAppKey;
                SigBaseStringParams += "&" + "oauth_nonce=" + Nonce.ToString();
                SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
                SigBaseStringParams += "&" + "oauth_timestamp=" + Math.Round(SinceEpoch.TotalSeconds);
                SigBaseStringParams += "&" + "oauth_version=1.0";
                String SigBaseString = "POST&";
                SigBaseString += Uri.EscapeDataString(DropboxUrl) + "&" + Uri.EscapeDataString(SigBaseStringParams);

                IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(DropboxAppSecret + "&",
                                                                                BinaryStringEncoding.Utf8);
                MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
                CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
                IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(SigBaseString,
                                                                                   BinaryStringEncoding.Utf8);
                IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
                String Signature = CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
                String Data = "OAuth oauth_callback=\"" + Uri.EscapeDataString(CallbackUrl) +
                              "\", oauth_consumer_key=\"" + DropboxAppKey + "\", oauth_nonce=\"" + Nonce.ToString() +
                              "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" +
                              Math.Round(SinceEpoch.TotalSeconds) + "\", oauth_version=\"1.0\", oauth_signature=\"" +
                              Uri.EscapeDataString(Signature) + "\"";

                HttpWebRequest Request = (HttpWebRequest) WebRequest.Create(DropboxUrl);
                Request.Method = "POST";
                Request.Headers["Authorization"] = Data;
                HttpWebResponse Response = (HttpWebResponse) await Request.GetResponseAsync();
                StreamReader ResponseDataStream2 = new StreamReader(Response.GetResponseStream());
                string m_PostResponse = await ResponseDataStream2.ReadToEndAsync();


                if (m_PostResponse != null)
                {
                    String oauth_token = null;
                    String oauth_token_secret = null;
                    String[] keyValPairs = m_PostResponse.Split('&');

                    for (int i = 0; i < keyValPairs.Length; i++)
                    {
                        String[] splits = keyValPairs[i].Split('=');
                        switch (splits[0])
                        {
                            case "oauth_token":
                                oauth_token = splits[1];
                                break;
                            case "oauth_token_secret":
                                oauth_token_secret = splits[1];
                                break;
                        }
                    }
                    if (oauth_token != null)
                    {

                        DropboxUrl = "https://www.dropbox.com/1/oauth/authorize?oauth_token=" + oauth_token + "&oauth_callback="+CallbackUrl;
                        System.Uri StartUri = new Uri(DropboxUrl);
                        System.Uri EndUri = new Uri(CallbackUrl);


                        WebAuthenticationResult WebAuthenticationResult =
                            await WebAuthenticationBroker.AuthenticateAsync(
                                WebAuthenticationOptions.None,
                                StartUri,
                                EndUri);

                        SinceEpoch = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                        Rand = new Random();
                        Nonce = Rand.Next(1000000000);

                        DropboxUrl = "https://api.dropbox.com/1/oauth/access_token";

                        SigBaseStringParams = "oauth_callback=" + Uri.EscapeDataString(CallbackUrl);
                        SigBaseStringParams += "&" + "oauth_consumer_key=" + DropboxAppKey;
                        SigBaseStringParams += "&" + "oauth_nonce=" + Nonce.ToString();
                        SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
                        SigBaseStringParams += "&" + "oauth_timestamp=" + Math.Round(SinceEpoch.TotalSeconds);
                        SigBaseStringParams += "&" + "oauth_token="+oauth_token;
                        SigBaseStringParams += "&" + "oauth_version=1.0";
                        SigBaseString = "POST&";
                        SigBaseString += Uri.EscapeDataString(DropboxUrl) + "&" + Uri.EscapeDataString(SigBaseStringParams);

                        KeyMaterial = CryptographicBuffer.ConvertStringToBinary(DropboxAppSecret + "&"+oauth_token_secret,
                                                                                        BinaryStringEncoding.Utf8);
                        HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
                        MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
                        DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(SigBaseString,
                                                                                           BinaryStringEncoding.Utf8);
                        SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
                        Signature = CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
                        Data = "OAuth oauth_callback=\"" + Uri.EscapeDataString(CallbackUrl) +
                                      "\", oauth_consumer_key=\"" + DropboxAppKey + "\", oauth_token=\""+oauth_token +"\", oauth_nonce=\"" + Nonce.ToString() +
                                      "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" +
                                      Math.Round(SinceEpoch.TotalSeconds) + "\", oauth_version=\"1.0\", oauth_signature=\"" +
                                      Uri.EscapeDataString(Signature) + "\"";

                            Request = (HttpWebRequest) WebRequest.Create(DropboxUrl);
                            Request.Method = "POST";
                            Request.Headers["Authorization"] = Data;
                      
                            Response = (HttpWebResponse) await Request.GetResponseAsync();

                        ResponseDataStream2 = new StreamReader(Response.GetResponseStream());
                            m_PostResponse = await ResponseDataStream2.ReadToEndAsync();


                            //if authorized, get access token
                            
                            if (m_PostResponse != null)
                            {
                                oauth_token = null;
                                oauth_token_secret = null;
                                keyValPairs = m_PostResponse.Split('&');

                                for (int i = 0; i < keyValPairs.Length; i++)
                                {
                                    String[] splits = keyValPairs[i].Split('=');
                                    switch (splits[0])
                                    {
                                        case "oauth_token":
                                            oauth_token = splits[1];
                                            break;
                                        case "oauth_token_secret":
                                            oauth_token_secret = splits[1];
                                            break;
                                    }
                                }
                                if (oauth_token != null)
                                {
                                    //get user info
                                    SinceEpoch = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                        Rand = new Random();
                        Nonce = Rand.Next(1000000000);

                                    string url = "https://api.dropbox.com/1/account/info";
                        
                        SigBaseStringParams = "oauth_consumer_key=" + DropboxAppKey;
                        SigBaseStringParams += "&" + "oauth_nonce=" + Nonce.ToString();
                        SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
                        SigBaseStringParams += "&" + "oauth_timestamp=" + Math.Round(SinceEpoch.TotalSeconds);
                        SigBaseStringParams += "&" + "oauth_token="+oauth_token;
                        SigBaseStringParams += "&" + "oauth_version=1.0";
                        SigBaseString = "GET&";
                        SigBaseString += Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(SigBaseStringParams);

                        KeyMaterial = CryptographicBuffer.ConvertStringToBinary(DropboxAppSecret + "&"+oauth_token_secret,
                                                                                        BinaryStringEncoding.Utf8);
                        HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
                        MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
                        DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(SigBaseString,
                                                                                           BinaryStringEncoding.Utf8);
                        SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
                        Signature = CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
                        Data = "?oauth_consumer_key=" + DropboxAppKey + "&oauth_token="+oauth_token +"&oauth_nonce=" + Nonce.ToString() +
                                      "&oauth_signature_method=HMAC-SHA1&oauth_timestamp=" +
                                      Math.Round(SinceEpoch.TotalSeconds) + "&oauth_version=1.0&oauth_signature=" +
                                      Uri.EscapeDataString(Signature);

                                    HttpClient client = new HttpClient();
                                    HttpResponseMessage res =
                            await client.GetAsync(url + Data);
                                    if (res.IsSuccessStatusCode)
                                    {
                                        Stream stream1 = await res.Content.ReadAsStreamAsync();
                                        StreamReader ResponseDataStream1 = new StreamReader(stream1);

                                        m_PostResponse = await ResponseDataStream1.ReadToEndAsync();

                                        if (m_PostResponse != null)
                                        {
                                            JsonObject jObject1 = JsonObject.Parse(m_PostResponse);
                                            string Email = "" + jObject1["email"].GetString();

                                            roamingSettings.CreateContainer("Accounts",
                                                                            ApplicationDataCreateDisposition.Always);

                                            if (roamingSettings.Containers.ContainsKey("Accounts"))
                                            {
                                                var composite = new ApplicationDataCompositeValue();
                                                string key = "2" + Email;
                                                composite["Type"] = "2";
                                                composite["Login"] = Email;
                                                composite["AccessToken"] = oauth_token;
                                                composite["TokenSecret"] = oauth_token_secret;

                                                roamingSettings.Containers["Accounts"].Values[key] = composite;

                                            }
                                        }
                                    }
                                }
                            }
                        
                    }
                }
            }
            catch
                (Exception Error)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                title = "Error";
                msg = Error.ToString();
            }

            //var messageDialog = new MessageDialog(msg, title);

            // await messageDialog.ShowAsync();

            if (roamingSettings.Containers.ContainsKey("Accounts") &&
                roamingSettings.Containers["Accounts"].Values.Count > 0)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        async private void Add_Box(object sender, RoutedEventArgs e)
        {
            string msg, title;
            try
            {
                //get code
                String BoxUrl = "https://api.box.com/oauth2/authorize?client_id=" + Uri.EscapeDataString(BoxClientID) + "&redirect_uri=" + Uri.EscapeDataString(BoxCallbackUrl) + "&response_type=code";


                System.Uri StartUri = new Uri(BoxUrl);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                System.Uri EndUri = new Uri(BoxCallbackUrl);


                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None,
                                                        StartUri,
                                                        EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {

                    //if authorized, get access token
                    string Code =
                        WebAuthenticationResult.ResponseData.Substring(WebAuthenticationResult.ResponseData.LastIndexOf("=") + 1);

                    HttpClient client = new HttpClient();

                    // client.DefaultRequestHeaders.Authorization =AuthenticationHeaderValue.Parse("OAuth" + data);

                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    postData.Add(new KeyValuePair<string, string>("client_secret", BoxClientSecret));
                    postData.Add(new KeyValuePair<string, string>("client_id", BoxClientID));
                    postData.Add(new KeyValuePair<string, string>("redirect_uri", BoxCallbackUrl));
                    postData.Add(new KeyValuePair<string, string>("code", Code));
                    HttpContent content = new FormUrlEncodedContent(postData);

                    HttpResponseMessage res = await client.PostAsync("https://api.box.com/oauth2/token", content);

                    if (res.IsSuccessStatusCode)
                    {
                        Stream stream = await res.Content.ReadAsStreamAsync();
                        StreamReader ResponseDataStream = new StreamReader(stream);

                        string jsonText = ResponseDataStream.ReadToEnd();

                        JsonObject jObject = JsonObject.Parse(jsonText);

                        string AccessToken = "" + jObject["access_token"].GetString();
                        string RefreshToken = "" + jObject["refresh_token"].GetString();
                        string TokenType = "" + jObject["token_type"].GetString();
                        string ExpiresIn = "" + jObject["expires_in"].GetNumber();

                        HttpResponseMessage res1 =
                            await client.GetAsync("https://api.box.com/2.0/users/me?access_token=" + AccessToken);
                        if (res1.IsSuccessStatusCode)
                        {
                            Stream stream1 = await res1.Content.ReadAsStreamAsync();
                            StreamReader ResponseDataStream1 = new StreamReader(stream1);

                            string jsonText1 = ResponseDataStream1.ReadToEnd();

                            JsonObject jObject1 = JsonObject.Parse(jsonText1);
                            string Email = "" + jObject1["login"].GetString();

                            roamingSettings.CreateContainer("Accounts", ApplicationDataCreateDisposition.Always);

                            if (roamingSettings.Containers.ContainsKey("Accounts"))
                            {
                                var composite = new ApplicationDataCompositeValue();
                                string key = "3" + Email;
                                composite["Type"] = "3";
                                composite["Login"] = Email;
                                composite["RefreshToken"] = RefreshToken;
                                composite["AccessToken"] = AccessToken;
                                composite["ExpiresIn"] = ExpiresIn;
                                composite["TokenType"] = TokenType;

                                roamingSettings.Containers["Accounts"].Values[key] = composite;

                            }


                        }

                    }

                    //call user api, store in roaming settings
                    //todo store credentials
                    //store in roaming settings
                    msg = "Account added successfully.";
                    title = "Success!";


                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseErrorDetail.ToString();

                }
                else
                {
                    title = "Error";
                    msg = WebAuthenticationResult.ResponseStatus.ToString();
                }

            }
            catch (Exception Error)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                title = "Error";
                msg = Error.ToString();
            }

            //var messageDialog = new MessageDialog(msg, title);

            // await messageDialog.ShowAsync();

            if (roamingSettings.Containers.ContainsKey("Accounts") &&
                roamingSettings.Containers["Accounts"].Values.Count > 0)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

    }
}
