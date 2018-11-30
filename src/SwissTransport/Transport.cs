﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SwissTransport
{
    public class Transport : ITransport
    {
        public Stations GetStations(string query)
        {
            var request = CreateWebRequest("http://transport.opendata.ch/v1/locations?type=station&query=" + query);


            return JsonConvert.DeserializeObject<Stations>(Get(request),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public StationBoardRoot GetStationBoard(string id, DateTime departureDateTime)
        {
            var request = CreateWebRequest($"http://transport.opendata.ch/v1/stationboard?id={id}&datetime={departureDateTime:yyyy-MM-dd HH:mm}");

            return JsonConvert.DeserializeObject<StationBoardRoot>(Get(request));
        }

        public Connections GetConnections(string fromStation, string toStation, int connectionsCount, DateTime departureDateTime)
        {
            var request =
                CreateWebRequest(
                    $"http://transport.opendata.ch/v1/connections?from={fromStation}&to={toStation}&limit={connectionsCount}&date={departureDateTime:yyyy-MM-dd}&time={departureDateTime:HH:mm}");

            return JsonConvert.DeserializeObject<Connections>(Get(request),
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

        public string Get(WebRequest webRequest)
        {
            var request = (HttpWebRequest)webRequest;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
