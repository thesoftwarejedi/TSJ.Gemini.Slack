Gemini -> Slack integration
================

Provides slack integration by posting issue changes to a channel in slack.  This includes every comment, assignment, resolution, satus chnge, and create.

To install, download release and place in gemini's App_Data/Apps folder.  Restart the web app (maybe a better way?  I couldn't find one).

To configure:
- Sign into your slack account (you must be an admin)
- Enable Slack Incoming Web Hooks https://www.slack.com/services/new/incoming-webhook
- On the incoming web hooks page, scroll down and choose a channel (any, doesn't matter), then click "create web hook"
- In Gemini, click "customize" up top
- Click "apps" on the top
- Enable Slack Integration
- Click the "Slack Integration" tab on the left
- Add the slack url to the page, and press "Save"
- For each project, enter a channel name and press save (between each, awkward I know)
- Done!  Eat ice cream

Contact me via my info on github or @thesoftwarejedi on twitter with feedback.

To build, compile then add project DLL, views foldes, and manifest file to a zip and place in gemini's App_Data/Apps folder as specified here: http://docs.countersoft.com/developing-custom-apps/
