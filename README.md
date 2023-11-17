Application provide bots for game https://www.tribalwars.net/en-dk/

Done for a Polish language version: https://www.plemiona.pl/

So to use it in any other you will need to adjust some paths.

<br>



Application can work with 3 bots running at once. 

When time  passes each bot will run its iteration.
<br>
<br>

Modify application behavior setting variables in <i>appsettings.json</i>

Set flag <i>seg__enabled</i> to false if you are not planning to see detailed logs in installed local Seq.

<i>ChromeTabStartOfUrl</i> bot will run on a tab that url starts from what you provide.

<i>FarmSwitchPages</i> run but just on the first page or all of the farm assistant.

<i>IntervalMin/IntervalMax</i> next bot iteration will happen after random value in minutes between provided. Set both to the same if you want to run them in exact intervals. 

Application have <i>Time.Delay</i> in many places to let's say imitate human behavior.

<b>Created 'on a feet' so feel free to adjust and modify to your needs.</b>


<br>
<h3>Chrome</h3>
Bots work with Chrome browser to allow them to work you need to start Chrome in debug mode so:

1. Close running Chrome instances.
2. Run this from command line:
   <i>start chrome --remote-debugging-port=9222</i>
3. If bots don't work, close all other tabs and try again; restore tabs after the first run.



<h2><b>Gathering Bot</b></h2>
For bot to work you will need to have gathering_assistant.js script in your shortcut bar with data title(when you hover mouse over) 'Zbierak'. Or name it as you want and change the value in application code. 

Bot is simply clicking the script then starting gathering for each level. So adjust gathering_assistant.js to your needs.

After sending troops it gets the longest one time and runs bot again after it passes.



<h2><b>Farming Bot</b></h2>
Two modes: Simple and Advanced. 

Simple is just sending farm attacks using template B with avoidance of non-green attacks and wall above '0'.

Advanced is demolishing walls and sending scouts after some conditions. 
