using System;
using System.Linq;
using Countersoft.Gemini.Extensibility.Events;
using Countersoft.Gemini.Extensibility.Apps;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Gemini;
using Countersoft.Gemini.Contracts;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Infrastructure.Managers;
using System.Text;
using Countersoft.Gemini.Commons.Permissions;

namespace TSJ.Gemini.Slack
{
    [AppType(AppTypeEnum.Event),
    AppGuid("ABBADABB-AD00-4151-A177-1F0529EEE7E1"),
    AppName("Slack Integration"),
    AppDescription("Provides slack integration by posting updates to gemini to a channel in slack.")]
    public class SlackListener : AbstractIssueListener
    {

        private static GlobalConfigurationWidgetData<SlackConfigData> GetConfig(GeminiContext ctx)
        {
            try
            {
                return ctx.GlobalConfigurationWidgetStore.Get<SlackConfigData>(AppConstants.AppId);
            }
            catch
            {
                return null;
            }
        }

        private static string GetProjectChannel(int projectId, Dictionary<int, string> projectChannels)
        {
            string channel = null;
            projectChannels.TryGetValue(projectId, out channel);
            if (channel.IsEmpty())
            {
                // Try and get the all projects channel.
                projectChannels.TryGetValue(0, out channel);
            }

            return channel;
        }

        public static string GetIssueKey(IssueEventArgs args)
        {
            var project = args.Context.Projects.Get(args.Entity.ProjectId);
            if (project == null) return string.Empty;
            return string.Concat(project.Code, '-', args.Entity.Id);
        }

        public override void AfterComment(IssueCommentEventArgs args)
        {            
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Issue.Project.Id, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} added a comment to <{1}|{2} - {3}>"
                                            ,args.User.Fullname, args.BuildIssueUrl(args.Issue), args.Issue.IssueKey, args.Issue.Title),
                                    "more details attached",
                                    "good",
                                    new[] { new { title = "Comment", value = StripHTML(args.Entity.Comment), _short = false } });

            base.AfterComment(args);
        }

        public override void AfterAssign(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;
            StringBuilder buffer = new StringBuilder();
            var usersCache = GeminiApp.Cache().Users;
            foreach (var userId in args.Entity.GetResources())
            {
                var user = usersCache.Find(u=> u.Id == userId);
                if (user != null)
                {
                    buffer.Append(user.Fullname);
                    buffer.Append(", ");
                }
            }

            if(buffer.Length > 0)
            {
                buffer.Remove(buffer.Length-2,2);
            }
            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} assigned <{1}|{2} - {3}> to {4}"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), GetIssueKey(args), args.Entity.Title, buffer));

            base.AfterAssign(args);
        }

        public override void AfterCreate(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} created <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), GetIssueKey(args), args.Entity.Title),
                                            "more details attached",
                                            "good",
                                            new[] { new { title = "Description", value = StripHTML(args.Entity.Description), _short = false } });

            base.AfterCreate(args);
        }       

        public override void AfterResolutionChange(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated resolution on <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), GetIssueKey(args), args.Entity.Title),
                                            "resolution changed",
                                            "good",
                                            new[] { new
                                            {
                                                title = "Resolution Change",
                                                value = args.Context.Meta.ResolutionGet(args.Previous.ResolutionId).Label + " -> " + args.Context.Meta.ResolutionGet(args.Entity.ResolutionId).Label,
                                                _short = true
                                            } });

            base.AfterResolutionChange(args);
        }

        public override void AfterStatusChange(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated status on <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), GetIssueKey(args), args.Entity.Title),
                                            "status changed",
                                            "good",
                                            new[] { new
                                            {
                                                title = "Status Change",
                                                value = args.Context.Meta.StatusGet(args.Previous.StatusId).Label + " -> " + args.Context.Meta.StatusGet(args.Entity.StatusId).Label,
                                                _short = true
                                            } });

            base.AfterStatusChange(args);
        }

        public override void AfterUpdateFull(IssueDtoEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Issue.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            var issueManager = GeminiApp.GetManager<IssueManager>(args.User);
            var userManager = GeminiApp.GetManager<UserManager>(args.User);
            var userDto = userManager.Convert(args.User);
            var changelog = issueManager.GetChangeLog(args.Issue, userDto, userDto, args.Issue.Entity.Revised.AddSeconds(-30));
            var fields = changelog
                                .Select(a => new
                                {
                                    title = a.Field,
                                    value = StripHTML(a.FullChange),
                                    _short = a.Entity.AttributeChanged != ItemAttributeVisibility.Description && a.Entity.AttributeChanged != ItemAttributeVisibility.AssociatedComments
                                });

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated issue <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Issue), args.Issue.IssueKey, args.Issue.Title),
                                            "details attached",
                                            "warning",
                                            fields.ToArray());

            base.AfterUpdateFull(args);
        }
        
        public static string StripHTML(string htmlString)
        {
            return Countersoft.Foundation.Utility.Helpers.HtmlHelper.ConvertHtmlToText2(htmlString).Replace((char)160, ' '); // Replace unicode NBSP with normal space as it breaks slack....
        }
    }
}

