# Certificate Setup

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
```

This will create a key with two parts, a public and private part. The public
part of the key is what you can give to other people to prove you signed
something (think like your signature). The private part of the key is saved
on your machine and should not be shared. The "cert" we created is a
way to share the public key with others.

However, this export certificate is not encoded as base64. To convert it,
you can use a program like `CertUtil`

```PowerShell
certutil -encode "C:\Users\admin\Desktop\$certname.cer"
    "C:\Users\admin\Desktop\base64_$certname.cer"
```

This file will look something like this:

```txt
-----BEGIN CERTIFICATE-----
<lots of data encoded as base64>
-----END CERTIFICATE-----
```

Take the data between the begin and end certificate blocks and copy it
into the cert field of the websocket transport.

Note, this is the public part of the key so it's ok to share with other users.
Sharing the public key is required for authentication, sharing the private
part of the key is a security risk.
