# FTPHashedUpdater
Syncs a Folder with an FTP-Server Directory. Uses Hashes to Update only changed Files.

Use:
1. Create a Hashfile.txt of your source data folder with the Hashmapper(included in Project).
2. Upload the source data folder to your choosen FTP-Server and make sure the Hashfile is also included.
3. Open the FTPHashedUpdater, enter the server details and the local target directory and hit update.

Note:
This programm will add or replace local files with files from the FTP-Server, but it will not delete local files that are missing on the Server.

General information:
Coded this in 2015 for a Friend, who wanted a simple way to sync his clan's game mods over FTP. 
Since its no use to any one collecting dust on my drive i made it open source.
So if you want a Folder having the same content as a given directory on your FTP-Server and 
you also want to save bandwidth, this could be useful to you.

Potential risks:
The FTP-Credentials are saved as cleartext on your disk, so that FTP-Account you are using for this better has no write permission.
Any file not having the right hash will be replaced without warning.
