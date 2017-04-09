using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.Dht;
//using Sensors.OneWire.Common;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using Newtonsoft.Json.Linq;  //Add through Nuget  NewtoSoft.Json USE Version 9.1 not latest IMPORTANT
using System.Threading.Tasks;

namespace Sensors.DhtCS
{
    public sealed class DhtCS : IDhtCS
    {
        //Source: https://www.hackster.io/porrey/dht11-dht22-temperature-sensor-077790

        GpioPin OneWirePin = null;
        const int DHTPIN = 17;
        private IDht _dht = null;
        private List<int> _retryCount = new List<int>();
        private DateTimeOffset _startedAt = DateTimeOffset.MinValue;

        private ReaderWriterLockSlim dhtlock = null;

        public void InitDHT22()
        {
            dhtlock = new ReaderWriterLockSlim();
            GpioController controller = GpioController.GetDefault();

            if (controller != null)
            {
                OneWirePin = GpioController.GetDefault().OpenPin(DHTPIN, GpioSharingMode.Exclusive);
                _dht = new Dht22(OneWirePin, GpioPinDriveMode.Input);
                _startedAt = DateTimeOffset.Now;

            }
        }

        public void StopDHT22()
        {

            // ***
            // *** Dispose the pin.
            // ***
            if (OneWirePin != null)
            {
                OneWirePin.Dispose();
                OneWirePin = null;
            }

            // ***
            // *** Set the Dht object reference to null.
            // ***
            _dht = null;


        }

        public  string GetTempReadingsAsJSon()
        {

            JObject o = JObject.FromObject(Reading);
            string json = o.ToString();
            return json;
        }


        DhtReading _reading;
        public DhtReading Reading
        {
            get
            {
                dhtlock.EnterReadLock();
                try
                {
                    return _reading;
                }
                finally
                {
                    dhtlock.ExitReadLock();
                }
            }

            set
            {
                dhtlock.EnterWriteLock();
                try
                {
                    _reading = value;
                }
                finally
                {
                    dhtlock.ExitWriteLock();
                }
            }
        }
         
        public async Task ReadTempHumidity()
        {
            if (dhtlock == null)
                InitDHT22();
            Reading  = new DhtReading();           
            Reading = await _dht.GetReadingAsync().AsTask();
        }
    }
}

