﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SwissTransport
{
    public class Transport : ITransport
    {
        public async Task<Stations> GetStations(string query)
        {
            var request = CreateWebRequest($"http://transport.opendata.ch/v1/locations?query={query}");

            return JsonConvert.DeserializeObject<Stations>(await Get(request).ConfigureAwait(false),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public async Task<Stations> GetStations(Coordinate coordinates)
        {
            if (coordinates != null)
            {
                var request = CreateWebRequest("http://transport.opendata.ch/v1/locations?" +
                                               $"x={coordinates.XCoordinate}&" +
                                               $"y={coordinates.YCoordinate}");

                return JsonConvert.DeserializeObject<Stations>(await Get(request).ConfigureAwait(false),
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            }
            else
            {
                return null;
            }
        }

        public async Task<StationBoardRoot> GetStationBoard(string id, DateTime departureDateTime)
        {
            var request = CreateWebRequest("http://transport.opendata.ch/v1/stationboard?" +
                                           $"id={id}" +
                                           $"&datetime={departureDateTime:yyyy-MM-dd HH:mm}");

            return JsonConvert.DeserializeObject<StationBoardRoot>(await Get(request).ConfigureAwait(false),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public async Task<Connections> GetConnections(string fromStation, string toStation, int connectionsCount, DateTime departureDateTime, bool isArrivalTime = false)
        {
            var request =
                CreateWebRequest(
                    "http://transport.opendata.ch/v1/connections?" +
                    $"from={fromStation}&to={toStation}" +
                    $"&limit={connectionsCount}" +
                    $"&date={departureDateTime:yyyy-MM-dd}" +
                    $"&time={departureDateTime:HH:mm}" +
                    $"&isArrivalTime={(isArrivalTime ? "1" : "0")}");

            return JsonConvert.DeserializeObject<Connections>(await Get(request).ConfigureAwait(false),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static WebRequest CreateWebRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var webProxy = WebRequest.DefaultWebProxy;

            webProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            request.Proxy = webProxy;

            return request;
        }

        public async Task<string> Get(WebRequest webRequest)
        {
            var request = (HttpWebRequest)webRequest;

            using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}