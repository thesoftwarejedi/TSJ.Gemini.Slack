using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSJ.Gemini.Slack
{

    public class SlackConfigData
    {
        public string SlackAPIEndpoint { get; set; }
        public Dictionary<int, string> ProjectChannels { get; set; }

        public SlackConfigData()
        {
            SlackAPIEndpoint = null;
            ProjectChannels = new Dictionary<int, string>();
        }
    }
}
