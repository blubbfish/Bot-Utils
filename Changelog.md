## 1.2.2 - Going to netcore
### New Features
* Split Wbserver to AWebserver and AWebserverDataBackend
* Add mp4 as binary content
* make a mirror of this repo on github
### Changes
* change to c+ newer coding style
* mograde to c# netcore 3.1

## 1.2.1
### New Features
* Add LICENSE, CONTRIBUTING.md and README.md
### Bugfixes
* When using Dispose, kill also mqtt connection
### Changes
* A bit more debugging

## 1.2.0
### New Features
* Add MultiSourceBot*
* Refere MultiSourceBot, Webserver and Bot to it.
### Changes
* Refactor Bot to ABot
* Rewrite Mqtt module so that it not need to watch the connection.

## 1.1.9
### New Features
* Modify Output of SendFileResponse

## 1.1.8
### New Features
* Add logger to Webserver Class

## 1.1.7
### Changes
* Restrucutre loading, so that all is init and after the listener is started, REQUEST_URL_HOST gives now host and port

## 1.1.6
### New Features
* SendFileResponse with a parameter for the folder
* add function that parse post params

## 1.1.5
### New Features
* add a function to send an object as json directly

## 1.1.4
### New Features
* add Woff as Binary type

## 1.1.3
### Changes
* Variables parsing now as a String

## 1.1.2
### Bugfixes
* Fixing bug for Contenttype

## 1.1.1
### Changes
* Update to local librarys

## 1.1.0
### New Features
* Remove Helper from Bot-Utils