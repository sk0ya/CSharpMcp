using System.Text.RegularExpressions;

namespace FileSystem.Tools;

/// <summary>
/// .gitignoreファイルのパターンを解析し、ファイルやディレクトリの除外を管理するクラス
/// </summary>
public static class GitIgnoreParser
{
    /// <summary>
    /// 指定されたパスが除外パターンに一致するかを判定します
    /// </summary>
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

    /// <summary>
    /// 指定したディレクトリから.gitignoreパターンを読み込みます
    /// </summary>
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

    /// <summary>
    /// .gitignoreファイルからパターンを解析し、正規表現のリストとして返します
    /// </summary>
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

    /// <summary>
    /// Gitの除外パターンを正規表現に変換します
    /// </summary>
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

    /// <summary>
    /// 共通の除外パターンを取得します
    /// </summary>
    public static List<Regex> GetCommonIgnorePatterns()
    {
        var patterns = new List<Regex>
        {
            new Regex(@"^(?:.+/)?\.git(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^(?:.+/)?\.next(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new Regex(@"^(?:.+/)?bin(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^(?:.+/)?obj(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^(?:.+/)?target(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^(?:.+/)?dist(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^(?:.+/)?lib(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new Regex(@"^(?:.+/)?\.vs(?:/.*)?$", RegexOptions.IgnoreCase),
            new Regex(@"^.*\.user$", RegexOptions.IgnoreCase),
            new Regex(@"^.*\.suo$", RegexOptions.IgnoreCase),
            
            new Regex(@"^.*\.cache$", RegexOptions.IgnoreCase),
            
            new Regex(@"^.*~$", RegexOptions.IgnoreCase),
            new Regex(@"^.*\.bak$", RegexOptions.IgnoreCase),
            
            new Regex(@"^.*\.log$", RegexOptions.IgnoreCase),
            
            new Regex(@"^(?:.+/)?packages(?:/.*)?$", RegexOptions.IgnoreCase),
            
            new Regex(@"^(?:.+/)?node_modules(?:/.*)?$", RegexOptions.IgnoreCase)
        };

        return patterns;
    }
}