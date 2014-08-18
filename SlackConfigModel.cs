using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TSJ.Gemini.Slack
{

    public class SlackConfigModel
    {
        public string SlackUrl { get; set; }
        public IEnumerable<SelectListItem> Projects { get; set; }
        public string Channel { get; set; }
        public int SecondsToQueueChanges { get; set; }
    }

}
