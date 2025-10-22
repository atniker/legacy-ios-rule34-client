using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Rule34.Result;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using MonoTouch.UIKit;

namespace Rule34.Atnik
{
    class Rule34Controller
    {
        public static Dictionary<string, NSData> cachedPictures = new Dictionary<string, NSData>();

        public static string ApiKey
        {
            get 
            {
                return UserPreferences.GetString("api_key");
            }
            set 
            {
                UserPreferences.SetString("api_key", value);
            }
        }

        public static void Init() 
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                return true;
            };
        }

        async public static Task<List<List<CommentItem>>> GetComments(int post_id) 
        {
            List<List<CommentItem>> result = new List<List<CommentItem>>();
            var client = new WebClient();
            client.Headers.Add("Content-Type", "application/xml");
            string requestUri = string.Format("https://api.rule34.xxx/index.php?page=dapi&s=comment&q=index&xml=1&post_id={0}&api_key={0}", post_id.ToString(), ApiKey); 
            string rrequest = await client.DownloadStringTaskAsync(requestUri);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rrequest);
            XmlNodeList commentNodes = doc.SelectNodes("//comment");

            List<CommentItem> allComments = new List<CommentItem>();
            if (commentNodes != null)
            {
                foreach (XmlNode commentNode in commentNodes)
                {
                    string creator = commentNode.Attributes["creator"].Value; 
                    string body = commentNode.Attributes["body"].Value;

                    allComments.Add(new CommentItem(creator, body));
                }
            }

            for (int i = 0; i < allComments.Count; i += 10)
            {
                result.Add(allComments.Skip(i).Take(10).ToList());
            }

            return result;
        }

        async public static Task<Dictionary<string, string>> GetSuggestions(string query)
        {
            var result = new Dictionary<string, string>();

            if (query.Length > 0) 
            {
                var response = MiniJSON.Json.Deserialize(await new WebClient().DownloadStringTaskAsync(new Uri(string.Format("https://api.rule34.xxx/autocomplete.php?q={0}", query)))) as List<object>;

                foreach (var i in response)
                {
                    var item = (Dictionary<string, object>)i;
                    result[(string)item["value"]] = (string)item["label"];
                }
            }
            
            return result;
        }

        public static void SetImage(Action<NSData> callback, string url) 
        {
            if (cachedPictures.ContainsKey(url)) 
            {
                ResultViewController.instance.InvokeAsMain(() => { callback.Invoke(cachedPictures[url]); });
            }

            var request = new WebClient();
            request.DownloadDataAsync(new Uri(url));//.Replace("https://", "http://")));

            request.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs args) =>
            {
                if (args.Error != null)
                {
                    Console.WriteLine(args.Error);
                    return;
                }

                var data = NSData.FromArray(args.Result);

                while (cachedPictures.Count > 20)
                {
                    cachedPictures.Remove(cachedPictures.Keys.First());
                }

                cachedPictures[url] = data;
                ResultViewController.instance.InvokeAsMain(() => { callback.Invoke(data); });
            };
        }

        async public static Task<List<ImageItem>> GetPages(string query)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                MainViewController.instance.PromptKey(); 
                return null;
            }

            var results = new List<ImageItem>();
            var request = await new WebClient().DownloadStringTaskAsync(string.Format("https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&limit=1000&tags={0}&api_key={1}", Uri.EscapeDataString(query), ApiKey));

            System.Diagnostics.Debug.WriteLine(request);

            if (request == "\"Missing authentication. Go to api.rule34.xxx for more information\"")
            {
                MainViewController.instance.PromptKey();
                return null;
            }

            var data = MiniJSON.Json.Deserialize(request) as List<object>;

            foreach (var i in data)
            {
                var item = (Dictionary<string, object>)i;

                results.Add(new ImageItem(
                    (string)item["preview_url"],
                    (string)item["sample_url"],
                    Convert.ToInt32(item["score"]),
                    Convert.ToInt32(item["comment_count"]),
                    (string)item["file_url"],
                    Convert.ToInt32(item["id"]),
                    (string)item["tags"]
                ));
            }

            return results;
        }
    }
}
