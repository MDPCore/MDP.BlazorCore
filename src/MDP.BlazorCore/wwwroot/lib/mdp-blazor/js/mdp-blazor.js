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
                console.error("pageData=null, error=", error);                
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

    function invokeMethodAsync(arguments) {

        // invoke
        if (_interopComponent) {
            return _interopComponent.invokeMethodAsync("InvokeMethodAsync", arguments);
        } else {
            alert("DotNet reference not initialized.");
        }
    };


    // return
    return {
        initialize: initialize,
        invokeMethodAsync: invokeMethodAsync
    };
})();


// mdp.blazor.authenticationManager
mdp.blazor.authenticationManager = (function () {

    // methods
    function login(scheme, returnUrl) {

        // Redirect
        window.location.href = `/.auth/login/${scheme}?returnUrl=${encodeURIComponent(returnUrl)}`;
    }

    function logout(returnUrl) {

        // Redirect
        window.location.href = `/.auth/logout?returnUrl=${encodeURIComponent(returnUrl)}`;
    }


    // return
    return {
        login: login,
        logout: logout
    };
})();