ReceiverEndpoint

Exposes basic NSB handler functionally.

NSBBehaviourTest

Set of MSTests that exercise the behaviour expose via RecieverEndpoint

Create three SQL Server databases, Receiver, Shared and Sender.
Adjust the Connection Strings in the code and configs file as required.
Rebuild Solution in Visual Studio
Use CTRL+F5 to run the Endpoint in a console application. 
The tests can then be execute via Test Explorer in Visual Studio

Use the UseAzureTransport application configuration setting to switch between SQLTransport and AzureServiceBusTransport.
The relevant test cases will only run correctly when the appropriate transport is configured.

