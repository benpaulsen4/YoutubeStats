# YouTube Stats Collector
A powerful application to collect subscriber statistics for YouTube creators that allows you to organize them and generate insightful analytics.

## Setup
To use this application, you will need to define a `config.json` file that specifies general settings as well as the details of the YouTube creators to collect statistics for. An example config has been included in the repository, 
with comments explaining the format as well as demonstrate how the groups can be used to sort creators into logical groups and sub-groups.

> Note: You will require your own YouTube API v3 key, which can be obtained from Google directly.

As part of assembling a config file, you will need to obtain the channel ID of the creators you wish to collect stats for. Now that YouTube is transitioning to the new handle-based URLS (ie. youtube.com/@creator) it is a bit more difficult to get these IDs.
There are two main ways you can do it today:
1. Search the creator on Socialblade and copy the ID from there.
2. Go to a related channel page which links the creator you want the ID of in their "Channels" tab. For now, clicking on a channel linked in another creators page will use the old URLs which contain the ID in them directly. You can then copy the ID from that URL.

It is important that you **do not** use the channel handle here as the API only accepts the true IDs (which look like a bunch of random letters and numbers, see example config). If you find that a channel in your configuration is not having its data displayed in
any reports, its likely that the API cannot find the channel with the ID specified and so it was simply ignored.

## Usage
Once a `config.json` is created and placed in the same folder as the built executable (or the project folder for debugging), simply run the application and it will run based on this configuration. If you selected a CSV or Analytics report you should then also see 
a 'Results' folder is created with a subfolder for each group and associated data (including CSVs and graph PNGs).

If you encounter an error, its entirely possible that it could be a bug. Here are some potential errors and what they mean:
- "Config missing API key or incorrectly configured" (or similar): You have not included your YouTube API key or the key is not specified correctly in the config file
- "Config missing groups or incorrectly configured": The creators could not be read from your config file, you have probably formatted it incorrectly
- "statistics.SubscriberCount is null" (or similar): The API didn't return information for a channel you requested, make sure every ID is correct
- "Config missing report type or incorrectly configured" / "Unknown report type": You have not specified a report type to generate correctly
- "Unable to read/write file/folder" (or similar): Something prevented the app from accessing a data file in the Results folder, probably OS permissions related
- "Error generating <name> generation/group graph: Xs and Ys are not of the same length" (or similar): The charting library failed to read the data stored in the CSVs to generate this graph, probably because of a limitation in how files are read (see below)

### Limitations
The app is pretty full featured, but there are some known limitations of the Analytics reports:
- Data can not be (officially) imported - it is possible, but must be done manually and will probably break things until you iron out the data to adhere to the other limitations
- Creators cannot be renamed once they have had their initial data CSV generated (unless you also change it in the file)
- New creators can't be added to a sub-group once their CSV is created, or charts will break
- Creators can't be deleted from or rearranged in a sub-group unless you manually delete them from the CSV as well, or the CSV writer will write data in the wrong columns
- You can add new groups and sub-groups to existing groups once data has been collected, but don't rename them unless you also rename the files/folders
- All creators in a sub-group CSV must have a data entry for every single date that any other creator has an entry for, or charts will break

Basically, if you use the application to collect all data itself, and don't try to manually import data, it should be mostly fine. The only time these limitations will present themselves in normal use is if a creator's channel is deleted or similar.