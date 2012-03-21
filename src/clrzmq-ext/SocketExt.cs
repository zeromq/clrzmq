/*

    Copyright (c) 2011 Michael Compton <michael.compton@littleedge.co.uk>

    This file is part of clrzmq2.

    clrzmq2 is free software; you can redistribute it and/or modify it under
    the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    clrzmq2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ZMQ.ZMQExt {
    /// <summary>
    /// Socket Extension Methods
    /// </summary>
    public static class SocketExt {
        /// <summary>
        /// Receive all messages and seperate out socket identity and remove delimiter message
        /// </summary>
        /// <param name="skt">Socket</param>
        /// <param name="identity">Destination Socket Identity</param>
        /// <returns>Message Parts</returns>
        public static Queue<byte[]> RecvAll(this Socket skt, out byte[] identity) {
            Queue<byte[]> messages = skt.RecvAll();
            identity = messages.Dequeue();
            messages.Dequeue();
            return messages;
        }

        /// <summary>
        /// Receive all messages and seperate out socket identity and remove delimiter message
        /// </summary>
        /// <param name="skt">Socket</param>
        /// <param name="identity">Destination Socket Identity</param>
        /// <param name="encoding">Identity string encoding</param>
        /// <returns>Message Parts</returns>
        public static Queue<byte[]> RecvAll(this Socket skt, out string identity, Encoding encoding) {
            Queue<byte[]> messages = skt.RecvAll();
            identity = encoding.GetString(messages.Dequeue());
            messages.Dequeue();
            return messages;
        }

        /// <summary>
        /// Send Queue of message parts
        /// </summary>
        /// <param name="skt">Socket</param>
        /// <param name="data">Message Parts</param>
        public static void Send(this Socket skt, Queue<byte[]> data) {
            while (data.Count > 1) {
                skt.SendMore(data.Dequeue());
            }
            skt.Send(data.Dequeue());
        }

        /// <summary>
        /// Receive and deserialize an object of a given type using .Net binary serialization.
        /// </summary>
        /// <typeparam name="T">Received Object Type</typeparam>
        /// <param name="skt">Socket</param>
        /// <returns>T obj</returns>
        public static T Recv<T>(this Socket skt) {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream(skt.Recv()) { Position = 0 };
            var obj = (T)bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

        /// <summary>
        /// Receive and deserialize an object of a given type using .Net binary serialization.
        /// </summary>
        /// <typeparam name="T">Received Object Type</typeparam>
        /// <param name="skt">Socket</param>
        /// <param name="timeout">Non-blocking receive timeout</param>
        /// <returns>T obj</returns>
        public static T Recv<T>(this Socket skt, int timeout) where T : class {
            T obj = null;
            byte[] data = skt.Recv(timeout);
            if (data != null) {
                var bf = new BinaryFormatter();
                var ms = new MemoryStream(data) { Position = 0 };
                obj = (T)bf.Deserialize(ms);
                ms.Close();
            }
            return obj;
        }

        /// <summary>
        /// Serialize and send object of given type using .Net binary serialization
        /// </summary>
        /// <typeparam name="T">Sending Object Type</typeparam>
        /// <param name="skt">Socket</param>
        /// <param name="obj">T Object</param>
        public static void Send<T>(this Socket skt, T obj) {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            skt.Send(ms.ToArray());
            ms.Close();
        }
    }
}
