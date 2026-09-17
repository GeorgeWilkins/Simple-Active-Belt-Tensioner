# Software

There are two things you'll need to use this belt tensioner:
- [SimHub](https://www.simhubdash.com/) and a [License](https://www.simhubdash.com/get-a-license/) for 60FPS telemetry data
- The project's SimHub plugin, which must be copied into SimHub's installation directory

## Downloading SimHub

SimHub is going to already be on your machine or at least familiar to you if you have a sim rig. In case it's not, it's a popular utility in the Sim Racing community that collates and normalises telemtery data from various games and simulators, making it available to many devices and plugins. Our tensioner uses that telemetry to determine how much tension to apply to the belt at any given moment.

1. Go to [https://www.simhubdash.com/](https://www.simhubdash.com/) and click on `Download now` or the `Download` link at the top of the page
2. You'll see a large button, labelled something like `Download SimHub vX.X.X`, which will start the download when clicked
3. Unzip the downloaded file and run the executable within to install SimHub on your computer
4. Run SimHub at least once before attempting to install the plugin

## Licensing Simhub

The free version of SimHub offers 10Hz telemetry updates, which is sufficient for some devicces; but not for our tensioner. We'll need to buy a license to unlock 60Hz updates. It's cheap (as little as 8 EUR for a perpetual license) and absolutely worth the money.

Note that you do not need a _'Motion License'_ to use this tensioner; that is a more costly thing for full motion rigs. The regular license is sufficient for our needs.

1. Go to [https://www.simhubdash.com/](https://www.simhubdash.com/) and click on the `Get a license` link at the top of the page
2. Enter your details and complete payment
3. Grab the key from the e-mail you're sent and enter it into `Settings` > `General` > `Licenses` within SimHub
4. Once done you should see `Status: Licensed` at the bottom of the SimHub window

## Downloading & Installing The Plugin

1. Make sure _SimHub_ is closed before proceeding
2. Download the latest [SABT SimHub Plugin.zip](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/releases/latest/download/SABT.SimHub.Plugin.zip)
3. Unzip the downloaded file, which will contain a `.dll` file (the plugin itself) and a `/Languages/` folder
3. Copy both into your SimHub installation directory (e.g. `C:\Program Files\SimHub`)
4. Open _SimHub_, which should then recognise the plugin and load it
5. Click on the `Simple Active Belt Tensioner` plugin in the left-hand menu and configure it as you like

You can change the language of the plugin (and SimHub itself) from `Settings` > `General` > `Language`.

## Configuring The Plugin
The plugin contains instructions on how to initially set up the motors; a one-time process that uniquely identifies each motor so the plugin can send commands to each motor separately.

Beyond that, everything is documented within the plugin; including descriptions of the various sliders and options that control how the effects are applied.

![SABT Plugin](https://github.com/user-attachments/assets/1b047370-d27f-45a7-aa17-3123e560ef01)

### 📢 Important
_SimHub_ has an optional feature called _"Arduino"_, which may or may not be enabled in your installation. It is _completely unrelated to SABT_ and **not required** for SABT to work.

However if it is enabled, there is a good chance it will interfere with SABT by taking over the serial port presented by our motor controller.

You can check if it is enabled by clicking on `Add/remove features` within _SimHub_ and seeing if it is listed as an enabled plugin. Is it is, make sure `Show in left main menu` is toggled on, then find the `Arduino` menu item on the left-hand menu and open it.
 
To prevent it from causing problems, you can tell the _"Arduino"_ feature to stop scanning the SABT serial port, under the `ARDUINO SCAN SETTINGS` section as shown below.

Select the `Never scan selected ports` and toggle the numbered port that is labelled as `USB-Enhanced-SERIAL CH343`:

![Arduino Feature](https://github.com/user-attachments/assets/a03ee6c4-a3ab-4596-8b46-a9acc17f73fe)

Once you've done this, unplug the SABT controller's USB cable, close _SimHub_ and then reopen it; finally plugging back in the USB cable. Our plugin should then be able to see and communicate with the motor controller.

Unfortunately _SimHub_ plugins and features can cause conflicts with each other, including SABT. If you've tried the steps above and are still having communication issues, have a look a the [FAQ.md](FAQ.md) document and finally reach out via [a new discussion](/discussions/new/choose) or [create a new issue](/issues/new), describing your problem and including _SimHub_ logs.
