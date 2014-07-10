using System;
using System.Linq;
using Countersoft.Gemini.Extensibility.Events;
using Countersoft.Gemini.Extensibility.Apps;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
//using KellermanSoftware.CompareNetObjects;
using Countersoft.Gemini.Contracts;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Infrastructure.Managers;

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

        public override void AfterComment(IssueCommentEventArgs args)
        {            
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Issue.Project.Id, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} added a comment to <{1}|{2} - {3}>"
                                            ,args.User.Fullname, args.BuildIssueUrl(args.Issue), args.Issue.IssueKey, args.Issue.Title),
                                    "more details attached",
                                    "good",
                                    new[] { new { title = "Comment", value = StripHTML(args.Entity.Comment) } });

            base.AfterComment(args);
        }

        public override void AfterAssign(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Entity.ProjectId, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} assigned <{1}|{2} - {3}> to {4}"
                                            ,args.User.Fullname, args.BuildIssueUrl(args.Entity), args.Entity.Id, args.Entity.Title, args.Entity.Resources));

            base.AfterAssign(args);
        }

        public override void AfterCreate(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Entity.ProjectId, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} created <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), args.Entity.Id, args.Entity.Title));

            base.AfterCreate(args);
        }

        public override void AfterProgressUpdate(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Entity.ProjectId, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated progress on <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), args.Entity.Id, args.Entity.Title),
                                            "progress updated",
                                            "good",
                                            new[] { new
                                            {
                                                title = "Progress",
                                                value = args.Entity.PercentComplete,
                                                _short = true
                                            } });

            base.AfterProgressUpdate(args);
        }

        public override void AfterResolutionChange(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Entity.ProjectId, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated resolution on <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), args.Entity.Id, args.Entity.Title),
                                            "resolution changed",
                                            "good",
                                            new[] { new
                                            {
                                                title = "Resolution Change",
                                                value = args.Context.Meta.StatusGet(args.Previous.StatusId).Label + " -> " + args.Context.Meta.StatusGet(args.Entity.StatusId).Label,
                                                _short = true
                                            } });

            base.AfterResolutionChange(args);
        }

        public override void AfterStatusChange(IssueEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Entity.ProjectId, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated status on <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Entity), args.Entity.Id, args.Entity.Title),
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

        /*
        public override void AfterUpdateFull(IssueDtoEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel;
            data.Value.ProjectChannels.TryGetValue(args.Issue.Project.Id, out channel);
            if (channel == null || channel.Trim().Length == 0) return;

            var fields = new CompareLogic(new ComparisonConfig() { IgnoreUnknownObjectTypes = true, CompareChildren = true })
                                .Compare(args.Previous, args.Issue)
                                .Differences
                                .Select(a => new
                                {
                                    title = "1-" + a.PropertyName,
                                    value = a.Object1Value + " -> " + a.Object2Value,
                                    _short = true
                                });

            QuickSlack.Send(data.Value.SlackAPIEndpoint, channel, string.Format("{0} updated issue <{1}|{2} - {3}>"
                                            , args.User.Fullname, args.BuildIssueUrl(args.Issue), args.Issue.IssueKey, args.Issue.Title),
                                            "details attached",
                                            "warning",
                                            fields.ToArray());

            base.AfterUpdateFull(args);
        }
        */

        public static string StripHTML(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }
    }
}

