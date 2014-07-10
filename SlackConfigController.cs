using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Countersoft.Gemini.Commons.Dto;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Infrastructure.Apps;
using Countersoft.Foundation.Commons.Extensions;

namespace TSJ.Gemini.Slack
{
    public class AppConstants
    {
        public const string AppId = "ABBADABB-AD00-4151-A177-1F0529EEE7E1";
    }

    [AppType(AppTypeEnum.Config),
    AppGuid(AppConstants.AppId),
    AppControlGuid("D5ED7A38-BE89-403E-8118-E5C3CC8C8E71"),
    AppAuthor("Dana Hanna"),
    AppKey("SlackIntegration"),
    AppName("Slack Integration"),
    AppDescription("Provides slack integration by posting updates to gemini to a channel in slack."),
    AppRequiresConfigScreen(true)]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, NoStore = false, Location = OutputCacheLocation.None)]
    public class SlackConfigController : BaseAppController
    {
        public override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(null, "apps/slack/configure", new { controller = "SlackConfig", action = "GetConfig" });
            routes.MapRoute(null, "apps/slack/save", new { controller = "SlackConfig", action = "SaveConfig" });
        }

        public override WidgetResult Caption(IssueDto issue)
        {
            WidgetResult result = new WidgetResult();
            result.Success = true;
            result.Markup.Html = "Slack Integration";
            return result;
        }

        public override WidgetResult Show(IssueDto args)
        {
            var result = new WidgetResult();
            result.Success = true;
            result.Markup.Html = "Slack Integration";
            return result;
        }

        public override WidgetResult Configuration()
        {
            var model = new SlackConfigModel();

            try
            {
                var data = GeminiContext.GlobalConfigurationWidgetStore.Get<SlackConfigData>(AppConstants.AppId);
                if (data != null && data.Value != null)
                {
                    model.SlackUrl = data.Value.SlackAPIEndpoint;
                    if (data.Value.ProjectChannels != null && data.Value.ProjectChannels.ContainsKey(0))
                    {
                        model.Channel = data.Value.ProjectChannels[0];
                    }
                }
            } catch
            { }

            var projects = GeminiContext.Projects.GetAll();
            projects.Insert(0, new Project() { Id = 0, Name = GetResource(Countersoft.Gemini.ResourceKeys.AllProjects) });

            model.Projects = new SelectList(projects, "Id", "Name", 0);
            var result = new WidgetResult();
            result.Success = true;
            result.Markup = new WidgetMarkup("views/Settings.cshtml", model);
            return result;
        }

        public ActionResult SaveConfig(string SlackUrl, int project, string channel)
        {
            var data = GeminiContext.GlobalConfigurationWidgetStore.Get<SlackConfigData>(AppConstants.AppId);
            var saveData = data != null && data.Value != null ? data.Value : new SlackConfigData();
            saveData.SlackAPIEndpoint = SlackUrl;
            saveData.ProjectChannels[project] = channel;
            
            GeminiContext.GlobalConfigurationWidgetStore.Save<SlackConfigData>(AppConstants.AppId, saveData);
            return JsonSuccess();
        }

        public ActionResult GetConfig(int projectId)
        {
            var data = GeminiContext.GlobalConfigurationWidgetStore.Get<SlackConfigData>(AppConstants.AppId);
            string slack = "";
            string channel = "";
            if (data != null && data.Value != null)
            {
                slack = data.Value.SlackAPIEndpoint;
                if (data.Value.ProjectChannels.ContainsKey(projectId))
                {
                    channel = data.Value.ProjectChannels[projectId];
                }
            }
            return JsonSuccess(new { SlackUrl = slack, Channel = channel });
        }
    }
}
