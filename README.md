# WcfChatSample
Simple server-multiclient WCF sample app. Using SQLite EF6 almost "code-first" and *netTcpBinding* push notifications to send new messages

It consists of 3 components:

### WcfChatSample.Service
It`s a chat server WCF service. Processes clients commands, push/store messages, validate users

### WcfChatSample.Server
Server host app. Lanch WcfChatSample service and accept connections. Using SQLite DB to store messages and user logins (DB will be created if no exists or model change). Using *netTcpBinding* as transport.

Set admin credentials in app .config as follow:
```
<add key="AdminUsername" value="..." />
<add key="AdminPassword" value="..." />
```
### WcfChatSample.Client.Wpf
WPF client app. You can start many clients on same machine. Use admin account to disconnect users. On first connect you can login with any user credentials, when connect second time with same user, server will check user password.
