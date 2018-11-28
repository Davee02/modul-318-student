﻿using System;
using System.Collections.ObjectModel;
using SwissTransport.App.Helper;

namespace SwissTransport.App.ViewModel
{
    public class StationBoardViewModel : ViewModelBase
    {
        private readonly ITransport m_transport;

        private ObservableCollection<Station> m_stations = new ObservableCollection<Station>();
        private Station m_selectedStation = new Station();
        private DateTime m_selectedDateTime = DateTime.Now;
        private string m_searchText;
        private ObservableCollection<StationBoard> m_stationBoards = new ObservableCollection<StationBoard>();


        public ObservableCollection<StationBoard> StationBoards
        {
            get => m_stationBoards;
            set => SetProperty(ref m_stationBoards, value);
        }

        public DateTime SelectedDateTime
        {
            get => m_selectedDateTime;
            set
            {
                SetProperty(ref m_selectedDateTime, value);
                UpdateStationsBoard();
            }
        }

        public Station SelectedStation
        {
            get => m_selectedStation;
            set
            {
                m_selectedStation = value;
                UpdateStationsBoard();
            }
        }

        public ObservableCollection<Station> Stations
        {
            get => m_stations;
            set => SetProperty(ref m_stations, value);
        }

        public string SearchText
        {
            get => m_searchText;
            set
            {
                if (!string.Equals(value, m_searchText))
                {

                    Stations = m_transport.GetStations(value).StationList.ToObservableCollection();
                    SetProperty(ref m_searchText, value);
                }
            }
        }

        public StationBoardViewModel()
        {
            m_transport = new Transport();
        }

        private void UpdateStationsBoard()
        {
            if (SelectedStation?.Id != null)
            {
                StationBoards = m_transport.GetStationBoard(SelectedStation.Id).Entries.ToObservableCollection();
            }
        }
    }
}
