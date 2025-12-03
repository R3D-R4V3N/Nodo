using Microsoft.AspNetCore.Components;

namespace Rise.Client.Helpdesk.Components;

public partial class VideoComponent
{
    [Parameter] public string Titel { get; set; } = string.Empty;
    [Parameter] public string Url { get; set; } = string.Empty;

    private string ToEmbedUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "";

        Uri uri = new Uri(url);

        return uri.Host switch
        {
            string host when host.Contains("youtube.com") =>
                GetYoutubeLongFormEmbed(uri),

            string host when host.Contains("youtu.be") =>
                GetYoutubeShortFormEmbed(uri),
            _ => url
        };
    }

    private string GetYoutubeLongFormEmbed(Uri uri)
    {
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var v = query.Get("v");

        return string.IsNullOrEmpty(v)
            ? uri.ToString()
            : $"https://www.youtube.com/embed/{v}?rel=0&playsinline=1&modestbranding=1";
    }

    private string GetYoutubeShortFormEmbed(Uri uri)
    {
        var videoId = uri.AbsolutePath.Trim('/');
        return $"https://www.youtube.com/embed/{videoId}?rel=0&playsinline=1&modestbranding=1";
    }
}