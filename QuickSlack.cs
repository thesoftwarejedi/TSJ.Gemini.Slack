using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Countersoft.Foundation.Commons.Extensions;

namespace TSJ.Gemini.Slack
{
    public static class QuickSlack
    {
        public static void Send(string slackApiUrl, string channel, string text, string fallback = null, string color = null, object[] fields = null, string fieldsText = null){
            //this method could and should be a one-liner
            var o = new
            {
                channel = channel,
                username = "Gemini",
                link_names = 1,
                icon_emoji = ":traffic_light:",
                text = text,
                attachments = (fields != null && fields.Length > 0) ? new[] { 
                                        new 
                                        {
                                            fallback = fallback,
                                            text = fieldsText,
                                            //pretext = "",
                                            color = color,
                                            fields = fieldsText.HasValue() ? null : fields
                                            }
                                        }
                                        : null
            };
            var wc = new WebClient();
            wc.UploadString(new Uri(slackApiUrl),
                                o.ToJson().Replace("\"_short\"", "\"short\""));
        }
    }
}
