using System.Threading.Tasks;
using Sensors.Dht;

namespace Sensors.DhtCS
{
    public interface IDhtCS
    {
        DhtReading Reading { get; set; }

        string GetTempReadingsAsJSon();
        void InitDHT22();
        Task ReadTempHumidity();
        void StopDHT22();
    }
}