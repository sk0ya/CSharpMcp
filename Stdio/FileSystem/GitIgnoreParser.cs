using System.Text.RegularExpressions;

namespace FileSystem;

public static class GitIgnoreParser
{
    public static bool IsIgnored(string relativePath, List<Regex> ignorePatterns)
    {
        if (relativePath.Split('/').Any(part => part.StartsWith(".") && part != "." && part != ".."))
            return true;

        foreach (var pattern in ignorePatterns)
        {
            if (pattern.IsMatch(relativePath))
                return true;
        }

        return false;
    }

    public static List<Regex> LoadIgnorePatterns(string rootPath)
    {
        var patterns = new List<Regex>();

        patterns.AddRange(GetCommonIgnorePatterns());

        string gitignorePath = Path.Combine(rootPath, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            patterns.AddRange(ParseGitIgnore(gitignorePath, rootPath, rootPath));
        }

        return patterns;
    }

    public static List<Regex> ParseGitIgnore(string gitignorePath, string currentDir, string rootPath)
    {
        var patterns = new List<Regex>();
        var lines = File.ReadAllLines(gitignorePath);

        string relativeDir = Path.GetRelativePath(rootPath, currentDir).Replace("\\", "/");
        if (relativeDir == ".")
            relativeDir = "";

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("!"))
                continue;

            string regexPattern = ConvertGitWildcardToRegex(trimmedLine, relativeDir);
            patterns.Add(new Regex(regexPattern, RegexOptions.IgnoreCase));
        }

        return patterns;
    }

    private static string ConvertGitWildcardToRegex(string pattern, string relativeDir)
    {
        string result;
        bool dirOnly = pattern.EndsWith("/");
        if (dirOnly)
            pattern = pattern.TrimEnd('/');

        if (pattern.StartsWith("/"))
        {
            pattern = pattern.TrimStart('/');
            if (!string.IsNullOrEmpty(relativeDir))
            {
                result = relativeDir + "/" + pattern;
            }
            else
            {
                result = pattern;
            }
        }
        else
        {
            if (!pattern.Contains("/"))
            {
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    result = relativeDir + "/(?:.*/)?(" + pattern + ")";
                }
                else
                {
                    result = "(?:.*/)?(" + pattern + ")";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    result = relativeDir + "/" + pattern;
                }
                else
                {
                    result = pattern;
                }
            }
        }

        result = result
            .Replace(".", "\\.")
            .Replace("**/", "(?:.*/)?")
            .Replace("**", ".*")
            .Replace("*", "[^/]*")
            .Replace("?", "[^/]");

        if (dirOnly)
        {
            result += "(?:/.*)?";
        }
        else
        {
            result += "(?:$|/.*)";
        }

        return @"^" + result + @"$";
    }

    public static List<Regex> GetCommonIgnorePatterns()
    {
        var patterns = new List<Regex>
        {
            new(@"^(?:.+/)?\.git(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^(?:.+/)?\.next(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new(@"^(?:.+/)?bin(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^(?:.+/)?obj(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^(?:.+/)?target(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^(?:.+/)?dist(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^(?:.+/)?lib(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new(@"^(?:.+/)?\.vs(?:/.*)?$", RegexOptions.IgnoreCase),
            new(@"^.*\.user$", RegexOptions.IgnoreCase),
            new(@"^.*\.suo$", RegexOptions.IgnoreCase),
            
            new(@"^.*\.cache$", RegexOptions.IgnoreCase),
            
            new(@"^.*~$", RegexOptions.IgnoreCase),
            new(@"^.*\.bak$", RegexOptions.IgnoreCase),
            
            new(@"^.*\.log$", RegexOptions.IgnoreCase),
            
            new(@"^(?:.+/)?packages(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new(@"^(?:.+/)?node_modules(?:/.*)?$", RegexOptions.IgnoreCase)
        };

        return patterns;
    }
}