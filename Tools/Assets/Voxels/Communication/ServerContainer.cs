using UnityEngine;

namespace Voxels.Communication
{
    public class ServerContainer : MonoBehaviour
    {
        private Server? Server;

        public void StartServer()
        {
            if (Server != null)
            {
                Server.Dispose();
            }

            Server = new Server();
        }

        public void StopServer()
        {
            Server?.Dispose();
        }
    }
}