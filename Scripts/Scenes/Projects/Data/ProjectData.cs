using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Scenes.Projects.Data
{
    [Serializable]
    public class ProjectResponse
    {
        [JsonProperty("totalCount")]
        public int totalCount;

        [JsonProperty("data")]
        public List<ProjectData> dataList;
    }

    [Serializable]
    public class ProjectData
    {
        [JsonProperty("id")]
        public int id;

        [JsonProperty("title")]
        public string title;

        [JsonProperty("duration")]
        public string duration;

        [JsonProperty("department")]
        public string department;

        [JsonProperty("status")]
        public string status;

        [JsonProperty("mainTechStack")]
        public string mainTechStack;

        [JsonProperty("summary")]
        public string summary;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("imageSource")]
        public string imageSource;

        [JsonProperty("links")]
        public Dictionary<string, string> Links;
    }
}