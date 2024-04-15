using System.Net;
using UnityEngine;

namespace RealMode.Communication
{
    public static class DefaultAddress
    {
        public static IPAddress Address => IPAddress.Loopback;
        public const int Port = 9001;
    }

    public class ServerContainer : MonoBehaviour
    {
        public short Port
        {
            get => _port;
            set
            {
                _port = value;
                //Server.ChangePort(_port);
            }
        }

        public Server Server { get; private set; } = null!;

        public short _port = DefaultAddress.Port;

        private void Awake()
        {
            Server = new Server(_port, DefaultAddress.Address);
        }

        private void OnDestroy()
        {
            // called when the play mode is stopped
            Server?.Dispose();
        }
    }
}