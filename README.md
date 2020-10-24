# msfs2020-skyvector

A moving map for that connects MSFS 2020 to SkyVector.com 

## Status

[msfs2020-skyvector](https://github.com/MoMadenU/msfs2020-skyvector) currently being released as a beta. 

## Releases and Download

program zips releases are uploaded [here](/MoMadenU/msfs2020-skyvector/releases)

## Components

* [FSWebService](FSWebService/) local web service that uses simconnect to talk to the sim
* [SkyVectorMovingMap](SkyVectorMovingMap/) Chrome Extension that requests position information from the web service

## Install caveats

For this first release you must run the webservice on the same machine as the sim and **you must launch it as Administrator**

## Is it safe to run the web service on my machine ?

Steps have been taken to secure the use of the web service:
* Web service and sim must be on same machine (IP)
* A CORS header is in place to only allow Skyvector.com or localhost to use the seervice
* the service only supports a get and care has been taken to exclude anything suspicious such as injections


"This is a common occurrence, especially on Windows machines, and is almost always a false positive. Commercial virus scanning programs are often confused by the structure of Go binaries, which they don't see as often as those compiled from other languages."
