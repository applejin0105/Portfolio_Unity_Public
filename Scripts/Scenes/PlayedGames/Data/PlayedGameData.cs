using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Scenes.Projects.Data;

namespace Scenes.PlayedGames.Data
{
    [Serializable]
    public class PlayedGamesResponse
    {
        [JsonProperty("totalCount")]
        public int totalCount;

        [JsonProperty("data")]
        public List<ProjectData> dataList;
    }

    [Serializable]
    public class PlayedGamesData
    {
        [JsonProperty("id")]
        public int id;

        [JsonProperty("title")]
        public string title;

        [JsonProperty("score")]
        public int[] score;

        [JsonProperty("playTime")]
        public int playTime;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("imageSource")]
        public string imageSource;
    }
}