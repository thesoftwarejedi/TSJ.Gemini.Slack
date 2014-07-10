using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TSJ.Gemini.Slack
{
    public static class QuickSlack
    {
        public static async void Send(string slackApiUrl, string channel, string text, string fallback = null, string color = null, object[] fields = null){
            //this method could and should be a one-liner
            var o = new
            {
                channel = channel,
                username = "Gemini",
                icon_emoji = ":traffic_light:",
                text = text,
                attachments = (fields != null && fields.Length > 0) ? new[] { 
                                        new 
                                        {
                                            fallback = fallback,
                                            //text = "",
                                            //pretext = "",
                                            color = color,
                                            fields = fields
                                            }
                                        }
                                        : null
            };
            var js = new JavaScriptSerializer();
            var wc = new WebClient();
            await wc.UploadStringTaskAsync(new Uri(slackApiUrl),
                                js.Serialize(o).Replace("\"_short\"", "\"short\""));
        }
    }
}
