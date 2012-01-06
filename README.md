# clrzmq &mdash; Official 0MQ Bindings for .NET and Mono
*(Formerly clrzmq2; legacy bindings have moved to [clrzmq-old][clrzmq-old])*

This project aims to provide the full functionality of the underlying ZeroMQ API to CLR projects.

The version of libzmq currently being targeted is **2.1 (stable)**.

## Getting Started

The quickest way to get started with clrzmq is by using the [NuGet package][clrzmq-nuget-x86] ([64-bit version][clrzmq-nuget-x64]). The NuGet packages include a copy of the native libzmq.dll, which is required to use clrzmq.

You may also build clrzmq directly from the source. See the Development Environment Setup instructions below for more detail.

To get an idea of how to use clrzmq, have a look at the following example.

### Example server

```c#
using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZMQGuide
{
    class Program
    {
        static void Main(string[] args)
        {
            // ZMQ Context, server socket
            using (Context context = new Context(1)
            using (Socket socket = context.Socket(SocketType.REP))
            {
                socket.Bind("tcp://*:5555");
                
                while (true)
                {
                    // Wait for next request from client
                    string message = socket.Recv(Encoding.Unicode);
                    Console.WriteLine("Received request: {0}", message);

                    // Do Some 'work'
                    Thread.Sleep(1000);

                    // Send reply back to client
                    socket.Send("World", Encoding.Unicode);
                }
            }
        }
    }
}
```

### Example client

```c#
using System;
using System.Text;
using ZMQ;

namespace ZMQGuide
{
    class Program
    {
        static void Main(string[] args)
        {
            // ZMQ Context and client socket
            using (Context context = new Context(1))
            using (Socket requester = context.Socket(SocketType.REQ))
            {
                requester.Connect("tcp://localhost:5555");

                string request = "Hello";
                for (int requestNum = 0; requestNum < 10; requestNum++)
                {
                    Console.WriteLine("Sending request {0}...", requestNum);
                    requester.Send(request, Encoding.Unicode);

                    string reply = requester.Recv(Encoding.Unicode);
                    Console.WriteLine("Received reply {0}: {1}", requestNum, reply);
                }
            }
        }
    }
}
```

More C# examples can be found in the [0MQ Guide][zmq-guide] or in the [examples repository][zmq-example-repo]. Tutorials and API documentation specific to clrzmq are on the way.

## Development Environment

On Windows/.NET, clrzmq is developed with Visual Studio 2010. Mono development is done with MonoDevelop 2.8+.

### Windows/.NET

Before clrzmq can be used, the native `libzmq.dll` must be compiled for your target platform from [0MQ sources][libzmq-2].

#### libzmq

1. Download/clone the source from the [repository][libzmq-2] or the [ZeroMQ website][zmq-dl].
2. Open the Visual Studio solution, located in `builds/msvc/msvc.sln`.
3. Set the build configuration as necessary (e.g., Release/Win32 or WithOpenPGM/x64).
   * **NOTE:** WithOpenPGM builds require the bundled OpenPGM sources to be built for Windows, the steps of which are beyond the scope of this README.
4. Build the `libzmq` project.
5. `libzmq.dll` should now be located at `/lib` in the zeromq source tree.

#### clrzmq

1. Clone the source.
2. Run `nuget.cmd`, which downloads any dependent packages (e.g., Machine.Specifications for acceptance tests).
3. Copy `libzmq.dll` from the zeromq project to the appropriate location in the clrzmq lib folder (i.e., `/lib/x86` or `/lib/x64`).
4. Run `build.cmd` to build the project and run the test suite. PGM-related tests will fail if a non-PGM build of libzmq is used.
5. The resulting binaries will be available in `/build`.

### Mono

**NOTE**: Mono 2.10.7+ is required **for development only**, as the NuGet scripts and executables require this version to be present.
If you choose to install dependencies manually, you may use any version of Mono 2.6+.

#### Mono 2.10.7+ configuration

NuGet relies on several certificates to be registered with Mono. The following is an example terminal session (on Ubuntu) for setting this up correctly.
This assumes you have already installed Mono 2.10.7 or higher.

```shell
$ mozroots --import --sync

$ certmgr -ssl https://go.microsoft.com
$ certmgr -ssl https://nugetgallery.blob.core.windows.net
$ certmgr -ssl https://nuget.org
```

With any luck, this is the correct set of certificates to get NuGet working on Mono.

#### libzmq

Either clone the [ZeroMQ repository][libzmq-2] or [download the sources][zmq-dl], and then follow the build/install instructions for your platform.
Use the `--with-pgm` option if possible.

#### clrzmq

1. Clone the source.
2. Run `nuget.sh`, which downloads any dependent packages (e.g., Machine.Specifications for acceptance tests).
3. Run `build.sh` to build the project and run the test suite. PGM-related tests will fail if a non-PGM build of libzmq is used.
4. The resulting binaries will be available in `/build`.

## Issues

Issues should be logged on the [GitHub issue tracker][issues] for this project.

When reporting issues, please include the following information if possible:

* Version of clrzmq and/or how it was obtained (compiled from source, NuGet package)
* Version of libzmq being used
* Runtime environment (.NET/Mono and associated version)
* Operating system and platform (Win7/64-bit, Linux/32-bit)
* Code snippet demonstrating the failure

## Contributing

Pull requests and patches are always appreciated! To speed up the merge process, please follow the guidelines below when making a pull request:

* Create a new branch in your fork for the changes you intend to make. Working directly in master can often lead to unintended additions to the pull request later on.
* When appropriate, add to the AcceptanceTests project to cover any new functionality or defect fixes.
* Ensure all previous tests continue to pass (with exceptions for PGM tests)
* Follow the code style used in the rest of the project. ReSharper and StyleCop configurations have been included in the source tree.

Pull requests will still be accepted if some of these guidelines are not followed: changes will just take longer to merge, as the missing pieces will need to be filled in.

## License

This project is released under the [LGPL][lgpl] license, as is the native libzmq library. See LICENSE for more details as well as the [0MQ Licensing][zmq-license] page.

[clrzmq-old]: https://github.com/zeromq/clrzmq-old
[clrzmq-nuget-x86]: http://packages.nuget.org/Packages/clrzmq
[clrzmq-nuget-x64]: http://packages.nuget.org/Packages/clrzmq-x64
[libzmq-2]: https://github.com/zeromq/zeromq2-1
[zmq-guide]: http://zguide.zeromq.org/page:all
[zmq-example-repo]: https://github.com/imatix/zguide/tree/master/examples/C%23
[zmq-dl]: http://www.zeromq.org/intro:get-the-software
[zmq-license]: http://www.zeromq.org/area:licensing
[issues]: https://github.com/zeromq/clrzmq/issues
[lgpl]: http://www.gnu.org/licenses/lgpl.html