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

    /**
     * Messages are sent to slack on 3 events:
     * - Create
     *      - Immediately published to a slack channel
     *      
     * - Change (includes comments)
     *      - this create a thread (per user/ticket#) which will wait for X seconds 
     *          of no changes flushing out all changes made in that time period.  This is
     *          to accomodate several changes being made at the same time without flooding a channel.
     * */
    [AppType(AppTypeEnum.Event),
    AppGuid("ABBADABB-AD00-4151-A177-1F0529EEE7E1"),
    AppName("Slack Integration"),
    AppDescription("Provides slack integration by posting updates to gemini to a channel in slack.")]
    public class SlackListener : AbstractIssueListener
    {

        //not sure of scope here, is a listener treated as a singleton?  If it is, this need not be static
        //tuple is user and issueid
        private static Dictionary<Tuple<string, int>, IdleTimeoutExecutor> _executorDictionary =
            new Dictionary<Tuple<string, int>, IdleTimeoutExecutor>();

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
                                            new[] { new { title = "Description", value = StripHTML(args.Entity.Description), _short = false } }, 
                                            StripHTML(args.Entity.Description));

            base.AfterCreate(args);
        }       

        /***
         * the functionality here hinges on the "changelog" that is provided from the gemini api
         * We don't have to keep track of changes.
         * This method looks for a recent change for this user/issue and extends the timeout if there is
         * a match, otherwise it creates an executor to post to slack after 60 seconds
         * */
        public override void AfterUpdateFull(IssueDtoEventArgs args)
        {
            var data = GetConfig(args.Context);
            if (data == null || data.Value == null) return;

            string channel = GetProjectChannel(args.Issue.Entity.ProjectId, data.Value.ProjectChannels);
            if (channel == null || channel.Trim().Length == 0) return;

            lock (_executorDictionary)
            {
                var key = Tuple.Create(args.User.Username, args.Issue.Id);
                //look for an existing username/issue# combination indicating that a change was recently
                //made in which case we just extend the timeout
                IdleTimeoutExecutor ex = null;
                if (!_executorDictionary.TryGetValue(key, out ex))
                {
                    DateTime createDate = DateTime.Now.AddSeconds(-1);

                    _executorDictionary[key] = new IdleTimeoutExecutor(DateTime.Now.AddSeconds(data.Value.SecondsToQueueChanges),
                        //this executes x  seconds after the last update, initially set above  ^^  then adjusted on subsequent
                        //updates further below (in the else) based on the key being found
                        () => { PostChangesToSlack(args, data, channel, createDate); },
                        () => { _executorDictionary.Remove(key); }, 
                        _executorDictionary);
                }
                else
                {
                    //we found a pending executor, just update the timeout to be later
                    ex.Timeout = DateTime.Now.AddSeconds(data.Value.SecondsToQueueChanges);
                }
            }

            base.AfterUpdateFull(args);
        }

    //called when the timeout has expired which was waiting for pending changes.
    private static void PostChangesToSlack(IssueDtoEventArgs args, GlobalConfigurationWidgetData<SlackConfigData> data, string channel, DateTime createDate)
    {
        var issueManager = GeminiApp.GetManager<IssueManager>(args.User);
        var userManager = GeminiApp.GetManager<UserManager>(args.User);
        var userDto = userManager.Convert(args.User);
        var issue = issueManager.Get(args.Issue.Id);
        //get the changelog of all changes since the create date (minus a second to avoid missing the initial change)
        var changelog = issueManager.GetChangeLog(issue, userDto, userDto, createDate.AddSeconds(-1));
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
                                        "good", //todo colors here based on something
                                        fields.ToArray());
    }
        
        public static string StripHTML(string htmlString)
        {
            return Countersoft.Foundation.Utility.Helpers.HtmlHelper.ConvertHtmlToText2(htmlString).Replace((char)160, ' '); // Replace unicode NBSP with normal space as it breaks slack....
        }
    }
}

