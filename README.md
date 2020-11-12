





### Prevent HTTP caching

When a customer deploys an ARM template to the Azure portal via a link like `https://portal.azure.com/#create/Microsoft.Template/uri/...`, 
the Azure portal's backend system fetches the ARM template, and potentially UI definitions. The URI-encoded URLs in the fragment of the 
address point the concrete JSON files for a deployment. The Azure backend issues an unauthenticated HTTP GET to fetch the JSON files.

The Azure backend's HTTP client respects HTTP headers, like cache durations. The can become a problem in situations where an asset 
should not be cached. The `NoCacheAttribute` in the solution prevents Azure from caching. 