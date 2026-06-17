using System.Text;
using System.Xml.Linq;

public static class ResourceJsGenerator
{
    public static List<string> GenerateAllResourceJs(string resourcesRootPath)
    {
        var result = new List<string>();

        if (!Directory.Exists(resourcesRootPath))
            return result;

        // پوشه‌های سطح اول (HCM، Production، ...)
        var resourceFolders = Directory.GetDirectories(resourcesRootPath);

        foreach (var folder in resourceFolders)
        {
            var folderName = new DirectoryInfo(folder).Name;

            var resources = ReadAllResources(folder);

            if (!resources.Any())
                continue;

            var jsObjectName = $"resource{folderName}";

            var js = ToJsObject(jsObjectName, resources);

            result.Add(js);
        }

        return result;
    }

    // 🔹 خواندن همه resx ها با XML (Recursive)
    private static Dictionary<string, string> ReadAllResources(string folderPath)
    {
        var result = new Dictionary<string, string>();

        var resxFiles = Directory.GetFiles(
            folderPath,
            "*.resx",
            SearchOption.AllDirectories
        );

        foreach (var file in resxFiles)
        {
            // فایل‌های زبانی مثل fa.resx رو رد کن
            if (file.EndsWith(".fa.resx", StringComparison.OrdinalIgnoreCase))
                continue;

            var xml = XDocument.Load(file);

            var dataNodes = xml.Descendants("data");

            foreach (var node in dataNodes)
            {
                var key = node.Attribute("name")?.Value;
                var value = node.Element("value")?.Value;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                // جلوگیری از تکرار کلید
                if (!result.ContainsKey(key))
                    result.Add(key, value);
            }
        }

        return result;
    }

    // 🔹 تبدیل Dictionary به JS Object
    private static string ToJsObject(string objectName, Dictionary<string, string> resources)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"var {objectName} = {{");

        foreach (var item in resources)
        {
            var key = item.Key.ToLower();
            var value = item.Value?
                .Replace("\\", "\\\\")
                .Replace("'", "\\'");

            sb.AppendLine($"  {key}: '{value}',");
        }

        sb.AppendLine("};");

        return sb.ToString();
    }
}


//var resourcesRoot = Path.Combine(
//    Directory.GetCurrentDirectory(),
//    "Resources"
//);

//var jsObjects = ResourceJsGenerator.GenerateAllResourceJs(resourcesRoot);

//// همه JS ها با هم
//var allJs = string.Join("\n\n", jsObjects);
