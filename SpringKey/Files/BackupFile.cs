using SpringKey.Struct;
using System.IO;
using System.Text;

namespace SpringKey.Files;

public static class BackupFile
{
    public static List<UserInfo> GetUserList(string basePath)
    {
        var users = new List<UserInfo>();
        string linkPath = Path.Combine(basePath, "link");
        if (!Directory.Exists(linkPath))
            return users;

        foreach (string file in Directory.GetFiles(linkPath, "*.sklink"))
        {
            try
            {
                string hash = Path.GetFileNameWithoutExtension(file);
                string[] lines = File.ReadAllLines(file);
                if (lines.Length == 0) continue;

                string firstLine = lines[0].Trim();
                if (string.IsNullOrEmpty(firstLine)) continue;

                int spaceIdx = firstLine.IndexOf(' ');
                string userName = spaceIdx > 0 ? firstLine[..spaceIdx] : firstLine;
                users.Add(new UserInfo(hash, userName));
            }
            catch
            {
                continue;
            }
        }
        return users;
    }

    public static void ExportUser(string hash, string basePath, string outputPath)
    {
        var sb = new StringBuilder();
        string linkPath = Path.Combine(basePath, "link", $"{hash}.sklink");
        string linkValue = File.ReadAllText(linkPath, Encoding.UTF8);
        WriteSection(sb, "Link", $"{hash}.sklink", linkValue);

        string userPath = linkValue.Split(' ')[1];
        string indexPath = Path.Combine(userPath, ".skindex");
        WriteSection(sb, "index", ".skindex", File.ReadAllText(indexPath, Encoding.UTF8));

        string[] keyFiles = Directory.GetFiles(userPath, "*.skkey", SearchOption.AllDirectories);
        foreach (string keyFile in keyFiles)
        {
            if (Path.GetFileName(keyFile).Equals(".skindex", StringComparison.OrdinalIgnoreCase))
                continue;

            string keyValue = File.ReadAllText(keyFile, Encoding.UTF8);
            string dirName = Path.GetFileName(Path.GetDirectoryName(keyFile)) ?? string.Empty;
            string fileName = Path.GetFileName(keyFile);
            string fullKeyName = dirName + fileName;

            WriteSection(sb, "key", fullKeyName, keyValue);
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    public static void ImportUser(string basePath, string inputPath)
    {
        string content = File.ReadAllText(inputPath, Encoding.UTF8);
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Input file is empty");

        string? userPath = null;

        using var reader = new StringReader(content);
        string? currentSection = null;
        string? currentFileName = null;
        var contentLines = new List<string>();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) && trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                if (currentSection != null && currentFileName != null)
                    ProcessSection(currentSection, currentFileName, string.Join(Environment.NewLine, contentLines), basePath, ref userPath);

                currentSection = trimmed[1..^1];
                currentFileName = reader.ReadLine();
                if (currentFileName == null)
                    throw new InvalidDataException("Unexpected end of input after section header");
                contentLines.Clear();
            }
            else
            {
                contentLines.Add(line);
            }
        }

        if (currentSection != null && currentFileName != null)
            ProcessSection(currentSection, currentFileName, string.Join(Environment.NewLine, contentLines), basePath, ref userPath);
    }

    private static void WriteSection(StringBuilder sb, string section, string fileName, string value)
    {
        sb.AppendLine($"[{section}]");
        sb.AppendLine(fileName);
        if (section == "index")
            sb.AppendLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
        else
            sb.AppendLine(value);
    }

    private static void ProcessSection(string section, string fileName, string content, string basePath, ref string? userPath)
    {
        switch (section)
        {
            case "Link":
                string linkDir = Path.Combine(basePath, "link");
                Directory.CreateDirectory(linkDir);
                File.WriteAllText(Path.Combine(linkDir, fileName), content, Encoding.UTF8);

                int spaceIdx = content.IndexOf(' ');
                if (spaceIdx < 0)
                    throw new InvalidDataException("Invalid link file: missing space separator");
                userPath = content[(spaceIdx + 1)..].Trim();
                break;

            case "index":
                if (userPath == null)
                    throw new InvalidOperationException("Link section must precede index");
                byte[] indexData = Convert.FromBase64String(content);
                Directory.CreateDirectory(userPath);
                File.WriteAllText(Path.Combine(userPath, fileName), Encoding.UTF8.GetString(indexData), Encoding.UTF8);
                break;

            case "key":
                if (userPath == null)
                    throw new InvalidOperationException("Link section must precede key");
                if (fileName.Length < 2)
                    throw new InvalidDataException($"Invalid key file name: {fileName}");

                string dirName = fileName[..2];
                string keyFileName = fileName[2..];
                string keyDir = Path.Combine(userPath, dirName);
                Directory.CreateDirectory(keyDir);
                File.WriteAllText(Path.Combine(keyDir, keyFileName), content, Encoding.UTF8);
                break;
        }
    }
}
