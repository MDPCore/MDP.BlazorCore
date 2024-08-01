// mdp
var mdp = mdp || {};


// mdp.blazorCore
mdp.blazorCore = mdp.blazorCore || {};


// mdp.blazorCore.eventManager
mdp.blazorCore.eventManager = (function () {

    // methods
    function dispatchPageLoading() {

        // raise
        var pageLoadingEvent = new CustomEvent("MDPPageLoading", {
            detail: {}
        });
        document.dispatchEvent(pageLoadingEvent);
    }

    function dispatchPageLoaded() {

        // pageData
        var pageData = {};
        var pageDataElement = document.getElementById("mdp-blazor-core-pagedata");
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
        var pageErrorElement = document.getElementById("mdp-blazor-core-pageerror");
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
        var pageLoadedEvent = new CustomEvent("MDPPageLoaded", {
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


// mdp.blazorCore.errorManager
mdp.blazorCore.errorManager = (function () {

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


// mdp.blazorCore.scrollManager
mdp.blazorCore.scrollManager = (function () {

    // methods
    function initialize(scrollElement) {

        // require
        if (!scrollElement) scrollElement = window;

        // fields
        var _scrollTop = 0;
        var _scrollState = null;
        var _scrollThreshold = 100;

        // methods
        scrollElement.addEventListener("scroll", () => {
            
            // scrollTop
            var scrollTop = scrollElement.scrollY || scrollElement.scrollTop;
            if (!scrollTop) scrollTop = 0;
            if (Math.abs(_scrollTop - scrollTop) <= _scrollThreshold) return;
            
            // dispatch
            if (_scrollTop > scrollTop) {

                // scrollTop
                _scrollTop = scrollTop;

                // scrollState
                if (_scrollState == "scrollUpped") return;
                _scrollState = "scrollUpped";

                // scrollUppedEvent
                var scrollUppedEvent = new CustomEvent("MDPScrollUpped", {
                    detail: {
                        scrollElement: scrollElement
                    }
                });
                document.dispatchEvent(scrollUppedEvent);
            } else {

                // scrollTop
                _scrollTop = scrollTop;

                // scrollState
                if (_scrollState == "scrollDowned") return;
                _scrollState = "scrollDowned";

                // scrollDownedEvent
                var scrollDownedEvent = new CustomEvent("MDPScrollDowned", {
                    detail: {
                        scrollElement: scrollElement
                    }
                });
                document.dispatchEvent(scrollDownedEvent);
            }
        });
    }

    // return
    return {

        // methods
        initialize: initialize
    };
})();


// mdp.blazorCore.interopManager
mdp.blazorCore.interopManager = (function () {

    // fields
    var _interopComponent = null;


    // methods
    function initialize(interopComponent) {

        // default
        _interopComponent = interopComponent;
    }

    function invokeAsync(actionName, actionParameters, isBackground = false) {

        // invokingEvent
        (function () {
            if (isBackground == false) {
                var invokingEvent = new CustomEvent("MDPActionInvoking", {
                    detail: {}
                });
                document.dispatchEvent(invokingEvent);
            }            
        })();

        // invokedEvent
        var invokedTask = function () {
            if (isBackground == false) {
                var invokedEvent = new CustomEvent("MDPActionInvoked", {
                    detail: {}
                });
                document.dispatchEvent(invokedEvent);
            }
        };

        // execute
        if (_interopComponent) {            

            // localInvoke
            return _interopComponent.invokeMethodAsync("InvokeAsync", actionName, actionParameters).then(function (response) {
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            }).finally(invokedTask);;
        }
        else {

            // remoteInvoke
            return mdp.blazorCore.httpClient.sendAsync("/.blazor/interop/invokeAsync", { "controllerUri": window.location.href, "actionName": actionName, "actionParameters": actionParameters }).then(function (response) {
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


// mdp.blazorCore.httpClient
mdp.blazorCore.httpClient = (function () {

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

    // pageLoading
    document.addEventListener("MDPPageLoading", function (event) {

        // style
        document.body.classList.add("mdp-page-loading");

        // scroll
        window.scrollTo({ top: 0, behavior: 'auto' });
    });

    // pageLoaded
    document.addEventListener("MDPPageLoaded", function (event) {

        // style
        document.body.classList.remove("mdp-page-loading");

        // scroll
        window.scrollTo({ top: 0, behavior: 'auto' });

        // error
        if (event.detail.pageError) {
            mdp.blazorCore.errorManager.raise(new Error(event.detail.pageError.message));
        } 
    });


    // actionInvoking
    document.addEventListener("MDPActionInvoking", function (event) {

        // style
        document.body.classList.add("mdp-action-invoking");
    });

    // actionInvoked
    document.addEventListener("MDPActionInvoked", function (event) {

        // style
        document.body.classList.remove("mdp-action-invoking");
    });


    // scrollUpped
    document.addEventListener("MDPScrollUpped", function (event) {

        // style
        document.body.classList.add("mdp-scroll-upped");
        document.body.classList.remove("mdp-scroll-downed");
    });

    // scrollDowned
    document.addEventListener("MDPScrollDowned", function (event) {

        // style
        document.body.classList.remove("mdp-scroll-upped");
        document.body.classList.add("mdp-scroll-downed");
    });


    // textarea.autoHeight
    document.addEventListener('MDPPageLoaded', function () {
        document.querySelectorAll('textarea').forEach(function (textareaElement) {

            // require
            if (!textareaElement) return;
            if (textareaElement.autoHeight) return;
            textareaElement.autoHeight = true;

            // style
            textareaElement.addEventListener('input', function () {
                this.style.height = 'auto';
                this.style.height = this.scrollHeight + 'px';
            });
            textareaElement.style.height = textareaElement.scrollHeight + 'px';
        });
    });
})();


/* ---------- native-script ---------- */
(function () {

    // error.toJSON
    Error.prototype.toJSON = function () {
        return {
            name: this.name,
            message: this.message
        };
    };
})();