# ImageShare

An easy way to display an image folder from a Nextcloud instance shared by a link and add images.
No account on the Nextcloud instance is required, just a folder shared by a link with you like:
https://cloud.example.com/s/abc1234defg

## Publishing

### Android
Build publishable, signed bundle
```
dotnet publish -f:net7.0-android -c:Release /p:AndroidSigningKeyPass=<<PASSWORD>> /p:AndroidSigningStorePass=<<PASSWORD>>
```

### iOS
Build publishable, signed archive
```
dotnet publish -f net8.0-ios ./ImageShare/ImageShare.csproj
```

Get provider
```
/Applications/Transporter.app/Contents/itms/bin/iTMSTransporter -m provider -u <username> -p <password>
```
At the end of the output you'll see something like:
```
Provider listing:
   - Long Name -              - Short Name -
1  <<ASC_PROVIDER_LONG_NAME>> <<ASC_PROVIDER>>
```
This will tell you the ASC_PROVIDER for the uplaod command.

Upload signed archive
```
/Applications/Transporter.app/Contents/itms/bin/iTMSTransporter -m upload -updateChannel latest -assetFile ./ImageShare/bin/Release/net8.0-ios/ios-arm64/publish/ImageShare.ipa -u <username> -p <password> -asc_provider <<ASC_PROVIDER>>
```

When the archive is done processing, visit https://appstoreconnect.apple.com/apps and select the ImageShare app. In the top left corner click on the "+" and add a new version. Fill "New in this version" für all languages and modify anything else you want to change. Scroll down to "Build" and click on the "+". Select the archive you just uploaded. The app store will complain about missing compliance. Next to status "Missing Compliance", select "Manage" and answer all questions.
When you are happy with all your changes, on the top right of the page, click "Save" and then "Add to Review". Now click on "Submit for App-Review".
Now wait for review.
