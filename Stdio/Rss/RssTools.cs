using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace RssTools;

[McpServerToolType]
public static class RssFeedProcessor
{
    [McpServerTool, Description("Processes multiple RSS feeds from command line arguments and formats their content as markdown links, ignoring the first item of each feed")]
    public static async Task<string> ParseRssFeeds()
    {
        var urls = Environment.GetCommandLineArgs().Skip(1).ToArray();
        
        if (urls.Length == 0)
        {
            return "No RSS URLs provided. Please provide at least one RSS URL.";
        }

        var sb = new StringBuilder();
        
        var tasks = urls.Select(async url => 
        {
            var sbUrl = new StringBuilder();
            sbUrl.AppendLine($"Results from RSS feed: {url}");
            sbUrl.AppendLine("--------------------------------------------------");
            
            var results = await ExtractRssFeedItems(url);
            if (results.Count == 0)
            {
                sbUrl.AppendLine("No items found or unable to parse the RSS feed.");
            }
            else
            {
                foreach (var (title, link) in results)
                {
                    sbUrl.AppendLine($"[{title}]({link})");
                }
            }
            
            sbUrl.AppendLine();
            return sbUrl.ToString();
        });

        var allResults = await Task.WhenAll(tasks);
        foreach (var result in allResults)
        {
            sb.Append(result);
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Extracts titles and URLs from an RSS feed, excluding the first entry
    /// </summary>
    /// <param name="rssUrl">The URL of the RSS feed to process</param>
    /// <returns>A list of tuples containing the title and URL of each feed item</returns>
    static async Task<List<(string title, string url)>> ExtractRssFeedItems(string rssUrl)
    {
        var results = new List<(string title, string url)>();
        
        if (LoadSyndicationFeed(rssUrl, out var feed))
        {
            // Skip the first item and process the rest
            var items = feed.Items.Skip(1);
            
            foreach (var item in items)
            {
                string title = item.Title?.Text ?? "No Title";
                string url = item.Links.FirstOrDefault()?.Uri?.ToString() ?? "";

                if (!string.IsNullOrEmpty(url))
                {
                    results.Add((title, url));
                }
            }
        }
        
        // Simulate async operation for consistency
        await Task.Delay(1);
        return results;
    }

    /// <summary>
    /// Loads a syndication feed from a specified URL with error handling
    /// </summary>
    /// <param name="rssUrl">The URL of the RSS feed to load</param>
    /// <param name="feed">The output syndication feed object when successful</param>
    /// <returns>True if the feed was successfully loaded, false if any errors occurred</returns>
    private static bool LoadSyndicationFeed(string rssUrl, out SyndicationFeed feed)
    {
        try
        {
            using var reader = XmlReader.Create(rssUrl);
            feed = SyndicationFeed.Load(reader);
            return feed != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading RSS feed: {ex.Message}");
            feed = null;
            return false;
        }
    }
}