# Hosting Netcode Project

The netcode project has two main parts, a client and a server.
If you are running on Windows, you can host both a client and a server together
in one application as a 'host'. But the web browser cannot host a server and
has to act as a client.

## Local Web Server

You can find an example of the netcode project hosted at
[nickmaltbie.com/OpenKCC/Netcode]

To emulate the environment on github, you need to host the
website as an https server.

The easiest way to do this on Windows I've found is to
host the website via IIS. See
[IIS Web Server Overview](https://learn.microsoft.com/en-us/iis/get-started/introduction-to-iis/iis-web-server-overview)

Make sure to install the tool via windows features and enable SSL and the
https protocol in the website configuration.

If you want to configure your own custom certificate, you can use the
[Certificate Setup](certificate-setup.md) for more information on how
to configure a custom certificate.

## Hosting a Server

To host a server of the project, either open it in the Unity Editor
or launch the project from a build executable. Select the option 'Start Host'
to start a client and server together. And select 'Start Server' to start
just a server.
