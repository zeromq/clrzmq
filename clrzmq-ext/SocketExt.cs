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

using System;
using ZMQ;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ZMQExt {
    /// <summary>
    /// Socket Extension Methods
    /// </summary>
    public static class SocketExt {
        
        /// <summary>
        /// Receive and deserialize an object of a given type using .Net binary serialization.
        /// </summary>
        /// <typeparam name="T">Received Object Type</typeparam>
        /// <param name="skt">Socket</param>
        /// <returns>T obj</returns>
        public static T Recv<T>(this Socket skt) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(skt.Recv());
            ms.Position = 0;
            T obj = (T)bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

        /// <summary>
        /// Serialize and send object of given type using .Net binary serialization
        /// </summary>
        /// <typeparam name="T">Sending Object Type</typeparam>
        /// <param name="skt">Socket</param>
        /// <param name="obj">T Object</param>
        public static void Send<T>(this Socket skt, T obj) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            skt.Send(ms.ToArray());
            ms.Close();
        }
    }
}
