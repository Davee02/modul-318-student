﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SwissTransport.App.Helper;
using System.Windows.Input;

namespace SwissTransport.App.ViewModel
{
    public class NearStationsViewModel : ViewModelBase
    {
        private readonly ITransport m_transport;
        private ObservableCollection<Station> m_nearStations = new ObservableCollection<Station>();
        private Coordinate m_foundCoordinates = new Coordinate();
        private Station m_selectedManualStation = new Station();
        private ObservableCollection<Station> m_stations = new ObservableCollection<Station>();
        private string m_searchText;

        public Station SelectedStation { get; set; }
        public string SearchText
        {
            get => m_searchText;
            set
            {
                if (!string.Equals(value, m_searchText))
                {

                    Stations = m_transport.GetStations(value).Result.StationList.ToObservableCollection();
                    SetProperty(ref m_searchText, value);
                }
            }
        }
        public ObservableCollection<Station> Stations
        {
            get => m_stations;
            set => SetProperty(ref m_stations, value);
        }

        public Station SelectedManualStation
        {
            get => m_selectedManualStation;
            set
            {
                m_selectedManualStation = value;
                FoundCoordinates = m_selectedManualStation.Coordinate;
                UpdateStations();
            }
        }

        public ObservableCollection<Station> NearStations
        {
            get => m_nearStations;
            set => SetProperty(ref m_nearStations, value);
        }

        public Coordinate FoundCoordinates
        {
            get => m_foundCoordinates;
            set => SetProperty(ref m_foundCoordinates, value);
        }

        public ICommand GetDevicePosition { get; set; }
        public ICommand ShowStation { get; set; }

        public NearStationsViewModel()
        {
            m_transport = new Transport();
            GetDevicePosition = new RelayCommand(x => SetNearStations());
            ShowStation = new RelayCommand(x => OpenGoogleMapsWithCoordinates(SelectedStation.Coordinate));
        }

        private async void SetNearStations()
        {
            await Task.Run(async () =>
                {
                    try
                    {
                        var geoLocator = new GeoLocator();
                        if (geoLocator.StartLocator() && geoLocator.HasPermissions)
                        {
                            int i = 0;
                            while (i < 20)
                            {
                                if (geoLocator.LocatorIsReady)
                                {
                                    FoundCoordinates = geoLocator.GetLocation();
                                    break;
                                }

                                await Task.Delay(200);
                                i++;
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                "Die Position konnte leider nicht ermittelt werden. Bitte aktivieren Sie die Positionsdienste in Ihren Windows-Einstellungen und versuchen Sie es in ein paar Sekunden erneut.",
                                "Positionierung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch
                    {
                        MessageBox.Show(
                            "Die Position konnte leider nicht ermittelt werden. Bitte aktivieren Sie die Positionsdienste in Ihren Windows-Einstellungen und versuchen Sie es in ein paar Sekunden erneut.",
                            "Positionierung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            );
            UpdateStations();
        }

        private async void UpdateStations()
        {
            var stations = await m_transport.GetStations(FoundCoordinates);
            if (stations == null || stations.StationList?.Count == 0)
            {
                MessageBox.Show(
                    $"In der Nähe der Koordinaten \"{FoundCoordinates}\" konnte leider keine Station gefunden werden. Versuchen Sie die Suche erneut.",
                    "Keine Station gefunden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                NearStations = stations.StationList.ToObservableCollection();
            }
        }

        private void OpenGoogleMapsWithCoordinates(Coordinate coordinates)
        {
            if (coordinates != null)
            {
                Process.Start(Helper.Helper.GetGoogleMapsLinkForCoordinates(coordinates));
            }
            else
            {
                MessageBox.Show(
                    "Die ausgewählte Station kann leider nicht angezeigt werden, da dafür keine Koordinaten gespeichert sind.",
                    "Station kann nicht angezeigt werden.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}