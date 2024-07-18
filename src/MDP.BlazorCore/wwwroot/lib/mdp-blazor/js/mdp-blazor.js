// mdp
var mdp = mdp || {};


// mdp.blazor
mdp.blazor = mdp.blazor || {};


// mdp.blazor.eventManager
mdp.blazor.eventManager = (function () {

    // methods
    function dispatchPageLoading() {

        // raise
        var pageLoadingEvent = new CustomEvent("BlazorPageLoading", {
            detail: {}
        });
        document.dispatchEvent(pageLoadingEvent);
    }

    function dispatchPageLoaded() {

        // pageData
        var pageData = {};
        var pageDataElement = document.getElementById("mdp-blazor-pagedata");
        if (pageDataElement) {
            var pageDataString = pageDataElement.getAttribute("data-value");
            if (pageDataString) {
                try {
                    pageData = JSON.parse(pageDataString);
                } catch (error) {
                    console.error("pageData=null, error=", error.message);
                }
            }
        }

        // pageError
        var pageError = {};
        var pageErrorElement = document.getElementById("mdp-blazor-pageerror");
        if (pageErrorElement) {
            var pageErrorString = pageErrorElement.getAttribute("data-value");
            if (pageErrorString) {
                try {
                    pageError = JSON.parse(pageErrorString);
                } catch (error) {
                    console.error("pageError=null, error=", error.message);
                }
            }
        }
        if (Object.keys(pageError).length === 0) pageError = null;

        // raise
        var pageLoadedEvent = new CustomEvent("BlazorPageLoaded", {
            detail: {
                pageData: pageData,
                pageError: pageError
            }
        });
        document.dispatchEvent(pageLoadedEvent);
    }


    // return
    return {

        // methods
        dispatchPageLoading: dispatchPageLoading,
        dispatchPageLoaded: dispatchPageLoaded
    };
})();


// mdp.blazor.errorManager
mdp.blazor.errorManager = (function () {

    // fields
    var handlers = [];


    // methods
    function addHandler(handler) {

        // add
        handlers.push(handler);
    }

    function removeHandler(handler) {

        // remove
        var index = handlers.indexOf(handler);
        if (index >= 0) handlers.splice(index, 1);
    }

    function raise(error) {
                
        // message
        const message = typeof error === 'object' ? JSON.stringify(error) : { message: error };

        // raise
        if (handlers.length > 0) {

            // handlers
            handlers.forEach(function (handler) {
                handler(message);
            });
        }
        else {

            // alert
            alert("Error= " + message);
        }
    }


    // return
    return {

        // methods
        addHandler: addHandler,
        removeHandler: removeHandler,
        raise: raise
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

    function invokeAsync(methodName, methodParameters, isBackground = false) {

        // invokingEvent
        (function () {
            if (isBackground == false) {
                var invokingEvent = new CustomEvent("BlazorInvoking", {
                    detail: {}
                });
                document.dispatchEvent(invokingEvent);
            }            
        })();

        // invokedEvent
        var invokedTask = function () {
            if (isBackground == false) {
                var invokedEvent = new CustomEvent("BlazorInvoked", {
                    detail: {}
                });
                document.dispatchEvent(invokedEvent);
            }
        };

        // execute
        if (_interopComponent) {            

            // localInvoke
            return _interopComponent.invokeMethodAsync("InvokeAsync", methodName, methodParameters).then(function (response) {
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            }).finally(invokedTask);;
        }
        else {

            // remoteInvoke
            return mdp.blazor.httpClient.sendAsync("/.blazor/interop/invoke", { "serviceUri": window.location.href, "methodName": methodName, "methodParameters": methodParameters }).then(function (response) {
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            }).finally(invokedTask);;
        }        
    };


    // return
    return {

        // methods
        initialize: initialize,
        invokeAsync: invokeAsync
    };
})();


// mdp.blazor.httpClient
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


/* ---------- mdp-blazor ---------- */
(function () {

    // BlazorPageLoading
    document.addEventListener("BlazorPageLoading", function (event) {

        // style
        document.body.classList.add("mdp-blazor-loading");

        // scroll
        document.querySelectorAll(".mdp-wrapper").forEach(function (wrapperElement) {
            wrapperElement.scrollTo({ top: 0, behavior: 'auto' });
        });
        window.scrollTo({ top: 0, behavior: 'auto' });
    });

    // BlazorPageLoaded
    document.addEventListener("BlazorPageLoaded", function (event) {

        // style
        document.body.classList.remove("mdp-blazor-loading");

        // scroll
        document.querySelectorAll(".mdp-wrapper").forEach(function (wrapperElement) {
            wrapperElement.scrollTo({ top: 0, behavior: 'auto' });
        });
        window.scrollTo({ top: 0, behavior: 'auto' });

        // error
        if (event.detail.pageError) {
            mdp.blazor.errorManager.raise(new Error(event.detail.pageError.message));
        } 
    });


    // BlazorInvoking
    document.addEventListener("BlazorInvoking", function (event) {

        // style
        document.body.classList.add("mdp-blazor-invoking");
    });

    // BlazorInvoked
    document.addEventListener("BlazorInvoked", function (event) {

        // style
        document.body.classList.remove("mdp-blazor-invoking");

    });
})();


/* ---------- script ---------- */
(function () {

    // Error
    Error.prototype.toJSON = function () {
        return {
            name: this.name,
            message: this.message
        };
    };
})();


