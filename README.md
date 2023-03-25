# ImageShare

An easy way to display an image folder from a Nextcloud instance shared by a link and add images.
No account on the Nextcloud instance is required, just a folder shared by a link with you like:
https://cloud.example.com/s/abc1234defg

## Android signing
```
dotnet publish -f:net7.0-android -c:Release /p:AndroidSigningKeyPass=<<PASSWORD>> /p:AndroidSigningStorePass=<<PASSWORD>>
```
