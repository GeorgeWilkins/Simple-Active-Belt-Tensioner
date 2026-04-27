# Software

There are two things you'll need to use this belt tensioner:
- [SimHub](https://www.simhubdash.com/) and a [Licesnse](https://www.simhubdash.com/get-a-license/) for 60FPS telemetry data
- This project's [Plugin](./Plugin) `DLL` file, which must be copied into SimHub's installation directory

That's it. Once you open SimHub after copying the plugin `DLL`, it should recognise the plugin and load it. You can then add the plugin to your SimHub dashboard and configure it as you like.

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

1. Go to [the latest release](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/releases) and download the `User.ActiveBeltTensioner.dll` file
2. Make sure _SimHub_ is closed before proceeding to the next step
3. Copy the `DLL` file within to your SimHub installation directory (e.g. `C:\Program Files\SimHub`)
4. Open _SimHub_, which should then recognise the plugin and load it
5. Click on the `Simple Active Belt Tensioner` plugin in the left-hand menu and configure it as you like
