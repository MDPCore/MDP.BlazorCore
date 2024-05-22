// namespace
var mdp = mdp || {};
mdp.blazor = mdp.blazor || {};


// mdp.blazor.eventManager
mdp.blazor.eventManager = (function () {

    // methods
    function dispatchPageLoaded() {

        // pageData
        var pageData = {};
        var pageDataElement = document.getElementById("mdp-blazor-pagedata");
        if (pageDataElement) {
            try {                
                pageData = JSON.parse(pageDataElement.getAttribute("data-value"));
            } catch (error) {
                console.error("pageData=null, error=", error.message);                
            }
        } else {
            console.log("pageDataElement=null");
        }

        // PageLoaded
        var pageLoadedEvent = new CustomEvent("BlazorPageLoaded", {
            detail: { pageData: pageData }
        });
        document.dispatchEvent(pageLoadedEvent);
    }


    // return
    return {

        // methods
        dispatchPageLoaded: dispatchPageLoaded
    };
})();


// mdp.blazor.interopManager
mdp.blazor.interopManager = (function () {

    // fields
    var _interopComponent = null;


    // methods
    function initialize(interopComponent) {

        // default
        _interopComponent = interopComponent;
    }

    function invokeAsync(path, payload) {

        // localInvoke
        if (_interopComponent) {
            return _interopComponent.invokeMethodAsync("InvokeAsync", path, payload).then(function (response) {
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            });
        } 

        // remoteInvoke
        return mdp.blazor.httpClient.sendAsync("/.blazor/interop/invoke", { "path": path, "payload": payload }).then(function (response) {
            if (response.succeeded == true) {
                return response.result;
            } else {
                throw new Error(response.errorMessage);
            }
        });
    };


    // return
    return {

        // methods
        initialize: initialize,
        invokeAsync: invokeAsync
    };
})();


// httpClient
mdp.blazor.httpClient = (function () {

    // methods
    function sendAsync(url, body, headers, method) {

        // headers
        if (headers == null) headers = {};
        if (headers["Content-Type"] == null) headers["Content-Type"] = "application/json";
        if (headers["Accept"] == null) headers["Accept"] = "application/json";

        // method
        if (method == null) method = "POST";

        // post
        var task = fetch(url, {
            method: method,
            headers: headers,
            body: JSON.stringify(body)
        });

        // response
        task = task.then(function (response) {
            return response.text().then(function (text) {

                // error
                if (!response.ok) {
                    var error = new Error(text);
                    error.statusCode = response.status;
                    error.content = response.statusText || getStatusText(response.status);
                    error.url = response.url;
                    throw error;
                }

                // null
                if (!text && text != 0) {
                    return {
                        statusCode: response.status,
                        content: "No Content"
                    };
                }

                // json
                try {
                    var content = JSON.parse(text);
                    if (typeof content == 'object' && content) {
                        return {
                            statusCode: response.status,
                            content: content
                        };
                    }
                } catch (e) { }

                // string
                return {
                    statusCode: response.status,
                    content: text.replace(/^\"|\"$/g, '').replace(/\\\"/g, '"')
                };
            })
        });

        // response.content
        task = task.then(function (response) {

            // content
            return response.content;
        });

        // return
        return task;
    }

    function getStatusText(statusCode) {

        // statusTextMap
        const statusTextMap = {
            200: 'OK',
            201: 'Created',
            202: 'Accepted',
            204: 'No Content',
            400: 'Bad Request',
            401: 'Unauthorized',
            403: 'Forbidden',
            404: 'Not Found',
            405: 'Method Not Allowed',
            500: 'Internal Server Error',
            501: 'Not Implemented',
            502: 'Bad Gateway',
            503: 'Service Unavailable',
            504: 'Gateway Timeout'
        };

        // return
        return statusTextMap[statusCode] || 'Unknown Status';
    }


    // return
    return {

        // methods
        sendAsync: sendAsync,
        getStatusText: getStatusText
    };
})();