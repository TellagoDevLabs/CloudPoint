CloudPoint
==========

Cloud Storage Options for SharePoint 2010 - specifically for large media files like videos, audios and images.

This component allows storage of Media Asstes inside or outside Sharepoint (Sharepoint List, File System FTP server, AmazonS3 or Azure Blob Storage).
It also provides a mechanism for validating file sizes, length and format, encoding videos and genearating thumbnail and poster images.

For more information about it plase read: http://dgoins.wordpress.com/2012/01/19/using-amazon-s3-as-a-media-hosting-option-for-sharepoint-2010 and http://weblogs.asp.net/spano/archive/2012/01/12/media-processing-component-for-sharepoint.aspx

Setup
==========

1. Install prerequisites:
	On the application server(s) which will run the timer job:
		- .Net Framework 4.0	
		- Expression Encoder 4 Pro - fully activated (To verify it is fully activated open the program and see the Help-About dialog. It must NOT say (without codecs) after the Microsoft Expression Encoder 4 Pro name)
	On all (front-end and application) servers:
		- Desktop Experience Feature
		- External Assemblies on the GAC. Run the Setup/GACAssemblies.bat script on each server to install.

2. Build the Tellago.SP.Media.Encoder project and place it's output (.exe and .exe.config files) in a folder on the app server running the timer job. Take note of this path as you will need it later for configuration.
	
3. Create a (shared) folder for temporary media storage that can be accessed from both the account running the web site application pool and the sharepoint timer service. Give read and write permissions to these accounts. Take note of the folder path as you will need it later for configuration.

4. Configure the component. Run the Setup/Populate-SPSM-ConfigStoreList.ps1 passing the site url as parameter. You can change the config values on the Setup/Data/01-SPSM-ConfigStore.xlsx file before running the script or on the SPSM-ConfigStore list after running it. You will need to change at least the following values:
	TempLocationFolder
	EncoderExePath
	Amazon or FTP credentials in case of selecting those storage methods. The default is to use SPLibrary.
	
5. Package and deploy the Tellago.SP.Common and Tellago.SP.Media projects.

6. Build and execute the Tellago.SP.MediaJobInstaller console application with the following parameters: "i <serverNameWhereTheJobWillRun> <webAppUrlWhereTheMediaAssetsListResides>"

7. Restart the Sharepoint 2010 Timer Service.

8. Navigate to http://<siteCollection>/Lists/SPSM-ConfigStore to verify the configuration.

9. Navigate to http://<siteCollection>/Lists/MediaAssets and test uploading an image, and audio file and a video.
