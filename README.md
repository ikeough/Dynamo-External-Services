# Dynamo External Services
Dynamo External Services provides interfaces and extensions for Dynamo which allow developers to build external service integrations into Dynamo. It is designed to support services which use OAuth authentication.

###Tokens
Access to services using OAuth requires access tokens. These tokens are provided as a result of the OAuth "flow" (see [OAuth Bible](http://oauthbible.com/) for a good explanation of this). Once received, the tokens are added to the `AccessTokens` collection. The dynamo extension responds to tokens being added to this collection by attempting to construct a client for the corresponding service. 

###Extensions
Dynamo External Services provides the `ExternalServicesExtension` and the `ExternalServicesViewExtension` for Dynamo. The extension provides the base behavior for responding to changes to the token collection and initializing the client. The view extension adds the "External Services" menu to Dynamo, which allows the user to select a registered service and login.

###Implementation
To create an external service integration with Dynamo, you need to provide an implementation of the `IExternalService` interface. Once complete, you must register the assembly containing this implementation, and all required dependencies, in the following folder structure. Take note of the naming of the assembly containing the implementation. It must be called `Dynamo.ExternalServices.Foo.dll` where "Foo" is the name of your service.

```
<dynamo root>    
    /services    
      /Foo  
        Dynamo.ExternalServices.Foo.dll  
```
