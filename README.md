# TVShowCleanup
A directory cleanup Windows Service, that monitors changes, and keeps a specified number of files/episodes.

# Installation
1. Create User for the Service to RunAs.
2. Grant that User R/W permissions to %programdata%\TVShowCleanup folder, if you want logging to work.
3. Run Install.bat file to register TVShowCleanup.exe as a Service.
4. When prompted enter the Username as "ComputerName\Username" and the "Password" used when creating the user in STEP 1.
5. Browse to %programdata%\TVShowCleanup and edit the Configuration.txt file with your shows, username, and password.

# Sample Config:
{

	"DirectoriesToCleanup": [
	"\\\\Plex-pc\\tv\\TV\\The Tonight Show Starring Jimmy Fallon",

	"\\\\Plex-pc\\tv\\TV\\Last Week Tonight with John Oliver",

	"\\\\Plex-pc\\tv\\TV\\Vice",

	"\\\\Plex-pc\\tv\\TV\\Real Time with Bill Maher",

	"\\\\Plex-pc\\tv\\TV\\The Ellen DeGeneres Show",

	"\\\\Plex-pc\\TV3\\TV\\Jimmy Kimmel Live",

	"\\\\Plex-pc\\tv3\\TV\\Vice News Tonight",

	"\\\\Plex-pc\\TV2\\TV\\Full Frontal with Samantha Bee"],

	"EnableFileWatcherCleanup": true,
	
	"EnableGlobalTimerCleanup": true,
	
	"HowOftenInMinutesToCleanup": 1440,
	
	"KeepFilesNewerThanThisNumberOfDays": 15,
	
	"MinimumNumberOfFilesToKeep": 10,
	"Username":"Plex",
	"Password":"PlexPassword"

}

# Visual Studio Extensions Required
Microsoft Visual Studio 2017 Installer Projects
Download: https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.MicrosoftVisualStudio2017InstallerProjects
