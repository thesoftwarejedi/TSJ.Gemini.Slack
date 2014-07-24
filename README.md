Gemini -> Slack integration
================

Provides slack integration by posting issue changes to a channel in slack.  This includes every comment, assignment, resolution, satus change, and create.

To install, download release and place in gemini's App_Data/Apps folder.  Restart the web app (maybe a better way?  I couldn't find one).

To configure:
- Sign into your slack account (you must be an admin)
- Enable Slack Incoming Web Hooks https://www.slack.com/services/new/incoming-webhook
- On the incoming web hooks page, scroll down and choose a channel (any, doesn't matter), then click "create web hook"
- Copy the provided URL
- In Gemini, click "customize" up top
- Click "apps" on the top
- Enable Slack Integration
- Click the "Slack Integration" tab on the left
- Add the slack incoming web hooks url to the box (that you copied above), and press "Save"
- For each project you wish to enable integration for, select it, enter a channel name, then press save (between each... awkward I know)
- Done!  Eat ice cream

Contact me via my info on github or @thesoftwarejedi on twitter with feedback.

UPDATE: a post build task exists to automatically generate an archive file in the output directory.

Leaving this here:
To build, compile then add project DLL, views foldes, and manifest file to a zip and place in gemini's App_Data/Apps folder as specified here: http://docs.countersoft.com/developing-custom-apps/
