﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.DhtCS;
using Sensors.OneWire.Common;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Sensors.OneWire
{
	public sealed partial class MainPage : BindablePage
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        private List<int> _retryCount = new List<int>();
        private DateTimeOffset _startedAt = DateTimeOffset.MinValue;

        private DhtCS.IDhtCS dhtCS;

        public MainPage()
        {
            this.InitializeComponent();
            dhtCS = new DhtCS.DhtCS();



        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dhtCS.InitDHT22();

            //Don't start time until the UX is up
            _timer.Tick += _timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();

            // ***
            // *** Dispose the hw.
            // ***
            dhtCS.StopDHT22();



            // ***
            // *** Set the Dht object reference to null.
            // ***
            dhtCS = null;



            base.OnNavigatedFrom(e);
        }

        private async void _timer_Tick(object sender, object e)

        {


            int val = this.TotalAttempts;
            this.TotalAttempts++;


            await dhtCS.ReadTempHumidity();
            Sensors.Dht.DhtReading reading = dhtCS.Reading;

            _retryCount.Add(reading.RetryCount);

            this.OnPropertyChanged(nameof(AverageRetriesDisplay));
            this.OnPropertyChanged(nameof(TotalAttempts));
            this.OnPropertyChanged(nameof(PercentSuccess));

           if (reading.IsValid)

            {
                this.TotalSuccess++;
                this.Temperature = Convert.ToSingle(reading.Temperature);
                this.Humidity = Convert.ToSingle(reading.Humidity);
                this.LastUpdated = DateTimeOffset.Now;
                this.OnPropertyChanged(nameof(SuccessRate));
            }

            this.OnPropertyChanged(nameof(LastUpdatedDisplay));

        }

        public string PercentSuccess
        {
            get
            {
                string returnValue = string.Empty;
                int attempts = this.TotalAttempts;
                if (attempts > 0)
                {
                    returnValue = string.Format("{0:0.0}%", 100f * (float)this.TotalSuccess / (float)attempts);
                }
                else
                {
                    returnValue = "0.0%";
                }
                return returnValue;
            }
        }

        private int _totalAttempts = 0;
        public int TotalAttempts
        {
            get
            {
                return _totalAttempts;
            }

            set
            {
                this.SetProperty(ref _totalAttempts, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private int _totalSuccess = 0;
        public int TotalSuccess
        {
            get
            {
                return _totalSuccess;
            }
            set
            {
                this.SetProperty(ref _totalSuccess, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private float _humidity = 0f;
        public float Humidity
        {
            get
            {
                return _humidity;
            }
            set
            {
                this.SetProperty(ref _humidity, value);
                this.OnPropertyChanged(nameof(HumidityDisplay));
            }
        }

        public string HumidityDisplay
        {
            get
            {
                return string.Format("{0:0.0}% RH", this.Humidity);
            }
        }

        private float _temperature = 0f;
        public float Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                this.SetProperty(ref _temperature, value);
                this.OnPropertyChanged(nameof(TemperatureDisplay));
            }
        }

        public string TemperatureDisplay
        {
            get
            {
                return string.Format("{0:0.0} °C", this.Temperature);
            }

        }

        private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
        public DateTimeOffset LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                this.SetProperty(ref _lastUpdated, value);
                this.OnPropertyChanged(nameof(LastUpdatedDisplay));
            }
        }

        public string LastUpdatedDisplay
        {
            get
            {
                string returnValue = string.Empty;
                TimeSpan elapsed = DateTimeOffset.Now.Subtract(this.LastUpdated);
                if (this.LastUpdated == DateTimeOffset.MinValue)
                {
                    returnValue = "never";
                }
                else if (elapsed.TotalSeconds < 60d)
                {
                    int seconds = (int)elapsed.TotalSeconds;
                    if (seconds < 2)

                   {
                        returnValue = "just now";
                    }
                    else
                    {
                        returnValue = string.Format("{0:0} {1} ago", seconds, seconds == 1 ? "second" : "seconds");
                    }
                }
                else if (elapsed.TotalMinutes < 60d)
                {
                    int minutes = (int)elapsed.TotalMinutes == 0 ? 1 : (int)elapsed.TotalMinutes;
                    returnValue = string.Format("{0:0} {1} ago", minutes, minutes == 1 ? "minute" : "minutes");
                }
                else if (elapsed.TotalHours < 24d)
                {
                    int hours = (int)elapsed.TotalHours == 0 ? 1 : (int)elapsed.TotalHours;
                    returnValue = string.Format("{0:0} {1} ago", hours, hours == 1 ? "hour" : "hours");
                }
                else
                {
                    returnValue = "a long time ago";
                }
                return returnValue;
            }
        }



        public int AverageRetries
        {
            get
            {
                int returnValue = 0;
                if (_retryCount.Count() > 0)
                {
                    returnValue = (int)_retryCount.Average();
                }
                return returnValue;
            }
        }

        public string AverageRetriesDisplay
        {
            get
            {
                return string.Format("{0:0}", this.AverageRetries);
            }
        }

        public string SuccessRate
        {
            get
            {
                string returnValue = string.Empty;
                double totalSeconds = DateTimeOffset.Now.Subtract(_startedAt).TotalSeconds;
                double rate = this.TotalSuccess / totalSeconds;
                if (rate < 1)
                {
                    returnValue = string.Format("{0:0.00} seconds/reading", 1d / rate);
                }
                else
                {
                    returnValue = string.Format("{0:0.00} readings/sec", rate);
                }
                return returnValue;
            }
        }
    }
   
}
