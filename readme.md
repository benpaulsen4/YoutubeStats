# YouTube Stats Collector
An application to collect subscriber statistics for YouTube creators and generate insightful analytics.

## Setup
To start, you will need to define a `config.json` file that specifies general settings as well as the details of the YouTube creators to collect statistics for. An example config has been included in the repository, 
with comments explaining the format as well as demonstrate how the groups can be used to sort creators into logical groups and sub-groups.

> Note: You will require your own YouTube API v3 key, which can be obtained from Google directly.

When filling out a config file, you can either provide YouTube channel IDs directly or use their YouTube handle (including the @). All YouTube handles will be translated to channel IDs on the first run of the app. 

## Usage
Once a `config.json` is created and placed in the same folder as the built executable (or the project folder for debugging), simply run the application and it will run based on this configuration. If you selected a CSV or Analytics report you should then also see 
a 'Results' folder is created with a subfolder for each group and associated data (including CSVs and graph PNGs).

### Console Formatting
As part of v2, the console report has been beautified for improved viewability and to allow for more analytics data in the same space. As part of this, certain Unicode elements have been introduced including emojis. Some consoles do not support Unicode by default 
(mostly on Windows), and as such you will see a warning when the report is printed to the console. If you are using Windows Terminal on Windows 10/11, you can enable Unicode output for PowerShell to make this warning disappear by following these steps:

1. Go to Language & Region settings [here](ms-settings:regionlanguage)
2. Click on 'Administrative language settings'
3. Click on 'Change system locale'
4. Click the 'Beta: Use Unicode UTF-8 for worldwide language support' checkbox and then click 'Ok'
5. You may need to restart, and then the console should support UTF-8 out of the box

### Errors
If you encounter an error, it could either be a bug or an issue with your configuration. Here are some potential errors and what they mean:
- "Config missing API key or incorrectly configured" (or similar): You have not included your YouTube API key or the key is not specified correctly in the config file
- "Config missing groups or incorrectly configured": The creators could not be read from your config file, you have probably formatted it incorrectly (see `config.example.json`)
- "statistics.SubscriberCount is null" (or similar): The API didn't return information for a channel you requested, make sure every ID is correct
- "Config missing report type or incorrectly configured" / "Unknown report type": You have not specified a report type to generate correctly
- "Unable to read/write file/folder" (or similar): Something prevented the app from accessing a data file in the Results folder, probably OS permissions related
- "Error generating <name> sub-group/group graph: Xs and Ys are not of the same length": The charting library failed to read the data stored in the CSVs to generate this graph, probably because of a limitation in how files are read (see 'Limitations')
- "Error generating <name> sub-group/group graph: Initialization failed": The charting library failed to draw the graph due to missing `System.Drawing.Common` dependency. This occurs on non-Windows operating systems and will be addressed in a future version

### Limitations
There are some known limitations of the Analytics reports:
- Data can not be (officially) imported - it is possible, but must be done manually and will probably break things until you iron out the data to adhere to the other limitations
- Creators cannot be renamed once they have had their initial data CSV generated (unless you also change it in the file)
- New creators can't be added to a sub-group once their CSV is created, or analytics will break
- Creators can't be deleted from a sub-group unless you manually delete them from the CSV as well
- You can add new groups and sub-groups to existing groups once data has been collected, but don't rename them unless you also rename the files/folders
- All creators in a sub-group CSV must have a data entry for every single date that any other creator has an entry for, or charts will break
- From v2 onwards, creators can not have the same name (even if they are in different groups/sub-groups) 

Basically, if you use the application to collect all data itself, and don't try to manually import data, you shouldn't have any issues. The only time these limitations will present themselves in normal use is if a creator's channel is deleted or similar.

## TODO
The following improvements will be implemented in a future version:
- [x] Translate channel handles to channel IDs automatically
  - As Youtube transitions away from publicly displaying IDs, it becomes harder to get them, this would greatly simplify things
- [x] Add 'Awards' feature
  - This will introduce podium style awards for the best performance across all groups for specific metrics
- [ ] Add 'Milestones' feature
  - This will highlight once a creator has surpassed a certain subscriber milestone
- [x] Allow writing analytics reports to text files
  - This will make storing historical copies of analytics simpler
- [ ] Implement a GUI for editing the config file
  - Extending on the `ConfigUpdater` utility, a full wizard to edit the config file would make misconfiguration more difficult
- [ ] Implement a GUI for viewing analytics
  - This is just a concept at this point, but as the graphing library supports WPF, a GUI could be created to make viewing analytics data far simpler