# Certificate Setup

*Note*: As of 08/03/23, this feature is currently broken.
See Unity Transport's docs for details on how to configure this if needed:
[https://docs-multiplayer.unity3d.com/transport/current/secure-connection/#generate-keys-and-certificates](https://docs-multiplayer.unity3d.com/transport/current/secure-connection/#generate-keys-and-certificates)

So, in order to get this program to work over https protocol, using
secure socket layer, you need to include the flag for a secure websocket.
Not too hard in theory, but in order to setup a secure socket layer
communication, you also need to create a signed certificate.

Now, a signed cert isn't too hard to create, but I haven't set one up
before and the library I'm using needs a base64 encoding. Base64 encoding
just means that save this big number as a blob of text using 64 unique
letters to encode the data.

I found a nifty guide on how to setup a public certificate for testing. Going
to write down the steps in case I need to repeat this in the future.

[How To Create Self Signed Certificate](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate)

```PowerShell
# Setup the certificate
## Replace {certificateName}
$certname = "{certificateName}" 
$cert = New-SelfSignedCertificate -Subject "CN=$certname" `
    -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable `
    -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256

# Export it to a .cer file.
Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\$certname.cer"

# Export the private key to a pfx file
$mypwd = ConvertTo-SecureString -String "1234" -Force -AsPlainText

Get-ChildItem `
    -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" `
    | Export-PfxCertificate -FilePath "C:\Users\admin\Desktop\$certname.pfx" `
        -Password $mypwd
```

## Connecting via HTTPS and SSL

If you want to connect to your server from an `https:` you must use the
`Secure Connection` option and provide a valid certificate `.pfx` file and
a password for that certificate when setting up the server.

This can be configured in the GUI for the client on the StartMenu screen.

When connecting from https, you must add the certificate to the list
of trusted certificates for your machine. This can be done if the
cert has a valid signature, but if you are configuring the cert
locally and just want to do some debug testing, you can add the
cert manually by navigating to the address

```txt
https://<server-address>:<port>/netcode
```

The default for this is [https://127.0.0.1:34182/netcode](https://127.0.0.1:34182/netcode)
and your browser will prompt you if you want to trust the cert.

If the cert is signed by an external trusted source you don't need to worry
about running into this error. Also, if you test without https or just
for localhost/127.0.0.1, you don't need to worry about this error either.
