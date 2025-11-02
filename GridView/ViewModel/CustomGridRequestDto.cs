using System.Text.Json.Serialization;

namespace GridView.ViewModel
{
    public class CustomGridRequestDto
    {
        [JsonPropertyName("searchTerm")]
        public string? SearchTerm { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

    }
}
