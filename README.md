# msfs2020-skyvector

A moving map that connects MSFS 2020 to SkyVector. A preview of it working can be found [Here](https://youtu.be/mP5gA6EbgHw)

## Status

[msfs2020-skyvector](https://github.com/MoMadenU/msfs2020-skyvector) is currently being released as a beta.
Only Google Chrome is being support at this time  

## Releases and Download

program zips releases are [here](https://github.com/MoMadenU/msfs2020-skyvector/releases)

## Components

* [FSWebService](https://github.com/MoMadenU/msfs2020-skyvector/FSWebService/) local web service that uses simconnect to talk to the sim
* [SkyVectorMovingMap](https://github.com/MoMadenU/msfs2020-skyvector/SkyVectorMovingMap/) Chrome Extension that requests position information from the web service

## Install caveats

For this first release you must run the webservice on the same machine as the sim and **you must launch it as Administrator**
The Chrome Extension has not been released to the Chrome store so you need to load it as an unpacked extension by enabling developer mode

## Is it safe to run the web service on my machine ?

Steps have been taken to secure the use of the web service:
* Web service, browser, and sim must be all on same machine (IP). It is very lightweight and should not cause any frame rate issues whatssoever. If it does, please let me know immediately. 
* A CORS header is in place to only allow Skyvector.com or localhost to use the service
* the service currently supports a GET only and the code will exclude anything suspicious such as injection attempts
* The default port is 8001 which you can change (see comprehensive install google doc). Please be sure the port you use is not open on your router

**Please Read the [Google Doc](https://github.com/MoMadenU/msfs2020-skyvector/blob/main/MSFS%202020%20SkyVector%20Moving%20map.docx)  for a comprehensive guide on how to install**
