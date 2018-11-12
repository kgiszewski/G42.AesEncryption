# G42.AesEncryption
A wrapper for the .net implementation of AES.

## Basic Usage

1) Generate a key:

`AesEncryptionHelper.GenerateCipherKey();`

You can store this key in the `app/web.config` in an `<appSetting/>`:

```
  <appSettings>
    <add key="AesCipherKey" value="WY6g5LnKXFea4SQ5KVMsda/cyKwMf2bDaiuPDdejL74=" />
  </appSettings>
```

You can also set this explicitly in code:

`AesEncryptionHelper.CipherKey = "mykey";`

**Safeguard your key, please do not store it in source control if at all possible!!**

If you lose your key, you will be unable to recover anything that is encrypted (that is sort of the point).

2) You can Encrypt a normal string which will yield a `base64` string. This is your cipher text:

```
var plainText = "https://www.nsa.gov/";

var cipherTextAsBase64 = _encryptionHelper.EncryptAsBase64(plainText);
```

3) You can decrypt the encrypted base64 string back:

```
var plainText = _encryptionHelper.DecryptAsBase64(cipherTextAsBase64);
```

4) There are overloads for streams:
```
var encryptedStream = _encryptionHelper.Encrypt(stream);
var decryptedStream = _encryptionHelper.Decrypt(encryptedStream);
```

## Nuget
Get it on Nuget: https://www.nuget.org/packages/G42.AesEncryptionHelper.


## Nerd Notes
1) Don't lost your key, you won't be able to recover it.
2) If you encrypt the same string twice, you will get two cipher text strings that are different. That is on purpose so that you cannot visually deduce that two cipher text strings contain the same information. This is due to the IV being random.
3) If using Azure, you can place your key in the Azure App Settings to avoid source control storage.
