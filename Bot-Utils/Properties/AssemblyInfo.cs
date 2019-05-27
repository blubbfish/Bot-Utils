using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
[assembly: AssemblyTitle("Bot-Utils")]
[assembly: AssemblyDescription("Bot-Utils are helpers for programming a bot")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("BlubbFish")]
[assembly: AssemblyProduct("Bot-Utils")]
[assembly: AssemblyCopyright("Copyright ©  2018 - 27.05.2019")]
[assembly: AssemblyTrademark("© BlubbFish")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("de-DE")]

// Durch Festlegen von ComVisible auf FALSE werden die Typen in dieser Assembly
// für COM-Komponenten unsichtbar.  Wenn Sie auf einen Typ in dieser Assembly von
// COM aus zugreifen müssen, sollten Sie das ComVisible-Attribut für diesen Typ auf "True" festlegen.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("bb7bfcb5-3db0-49e1-802a-3ce3eecc59f9")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder Standardwerte für die Build- und Revisionsnummern verwenden,
// indem Sie "*" wie unten gezeigt eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.2.0")]
[assembly: AssemblyFileVersion("1.2.0")]

/*
 * 1.1.0 Remove Helper from Bot-Utils
 * 1.1.1 Update to local librarys
 * 1.1.2 Fixing bug for Contenttype
 * 1.1.3 Variables parsing now as a String
 * 1.1.4 add Woff as Binary type
 * 1.1.5 add a function to send an object as json directly
 * 1.1.6 rename functions and make SendFileResponse with a parameter for the folder (default resources), 
 *       also put returntype boolean, add function that parse post params, if path is a dictionary try to load index.html
 * 1.1.7 Restrucutre loading, so that all is init and after the listener is started, REQUEST_URL_HOST gives now host and port
 * 1.1.8 Add logger to Webserver Class
 * 1.1.9 Modify Output of SendFileResponse
 * 1.2.0 Refactor Bot to ABot and refere MultiSourceBot, Webserver and Bot to it. Add MultiSourceBot. Rewrite Mqtt module so that it not need to watch the connection. 
 */
