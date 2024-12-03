/* ---------- global ---------- */

// error.toJSON
Error.prototype.toJSON = function () {
    return {
        name: this.name,
        message: this.message
    };
};


/* ---------- platform ---------- */

// mdp.blazorCore
window.mdp = window.mdp || {};

mdp.blazorCore = mdp.blazorCore || {};

 
// mdp.blazorCore.applicationManager
mdp.blazorCore.applicationManager = (function () {

    // methods
    function isInMaui() {

        // isInMaui
        var isInMaui = false;
        if (window.location.href.toLowerCase().startsWith("app://localhost/") == true) isInMaui = true;
        if (window.location.href.toLowerCase().startsWith("https://0.0.0.0/") == true) isInMaui = true;

        // return
        return isInMaui;
    }


    // return
    return {

        // methods
        isInMaui: isInMaui
    };
})();

document.addEventListener("DOMContentLoaded", function (event) {

    // style
    if (mdp.blazorCore.applicationManager.isInMaui() == true) {
        document.body.classList.add("mdp-maui-mode");
    }
    else {
        document.body.classList.add("mdp-web-mode");
    }    
});

// mdp.blazorCore.pageManager
mdp.blazorCore.pageManager = (function () {

    // methods
    function dispatchPageLoading() {

        // pageLoading
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

        // pageLoaded
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

document.addEventListener("MDPPageLoading", function (event) {

    // style
    document.body.classList.add("mdp-page-loading");
});

document.addEventListener("MDPPageLoaded", function (event) {

    // scroll
    window.scrollTo({ top: 0, behavior: 'auto' });
    
    // style
    document.body.classList.remove("mdp-page-loading");

    // error
    if (event.detail.pageError) {
        mdp.blazorCore.errorManager.dispatchError(new Error(event.detail.pageError.message));
    }
});

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
        if (isBackground == false) {
            var invokingEvent = new CustomEvent("MDPActionInvoking", {
                detail: {}
            });
            document.dispatchEvent(invokingEvent);
        }    

        // invoke
        if (_interopComponent) {            

            // localInvoke
            return _interopComponent.invokeMethodAsync("InvokeAsync", actionName, actionParameters).then(function (response) {

                // response
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            }).finally(function () {

                // invokedEvent
                if (isBackground == false) {
                    var invokedEvent = new CustomEvent("MDPActionInvoked", {
                        detail: {}
                    });
                    document.dispatchEvent(invokedEvent);
                }
            });
        }
        else {

            // remoteInvoke
            return mdp.blazorCore.httpClient.sendAsync("/.blazor/interop/invokeAsync", { "controllerUri": window.location.href, "actionName": actionName, "actionParameters": actionParameters }).then(function (response) {

                // response
                if (response.succeeded == true) {
                    return response.result;
                } else {
                    throw new Error(response.errorMessage);
                }
            }).finally(function () {

                // invokedEvent
                if (isBackground == false) {
                    var invokedEvent = new CustomEvent("MDPActionInvoked", {
                        detail: {}
                    });
                    document.dispatchEvent(invokedEvent);
                }
            });
        }        
    };


    // return
    return {

        // methods
        initialize: initialize,
        invokeAsync: invokeAsync
    };
})();

document.addEventListener("MDPActionInvoking", function (event) {

    // style
    document.body.classList.add("mdp-action-invoking");
});

document.addEventListener("MDPActionInvoked", function (event) {

    // style
    document.body.classList.remove("mdp-action-invoking");
});

// mdp.blazorCore.taskManager
mdp.blazorCore.taskManager = (function () {

    // methods
    function invokeAsync(task, taskParameters = {}, isBackground = false) {
        return new Promise((resolve, reject) => {
            try {

                // require
                if (typeof task !== 'function') throw new Error("Task must be a function");

                // invokingEvent
                if (isBackground === false) {
                    const invokingEvent = new CustomEvent("MDPTaskInvoking", {
                        detail: {}
                    });
                    document.dispatchEvent(invokingEvent);
                }

                // invoke
                requestAnimationFrame(() => {
                    setTimeout(() => {
                        Promise.resolve().then(() => task(taskParameters))
                            .then(resolve)
                            .catch(reject)
                            .finally(() => {

                                // invokedEvent
                                if (isBackground === false) {
                                    const invokedEvent = new CustomEvent("MDPTaskInvoked", {
                                        detail: {}
                                    });
                                    document.dispatchEvent(invokedEvent);
                                }
                            });
                    }, 0);
                });
            } catch (error) {

                // reject
                reject(error);
            }
        });
    }

    function invokeWorkerAsync(task, taskParameters = {}, isBackground = false) {
        return new Promise((resolve, reject) => {
            try {

                // require
                if (typeof task !== 'function') throw new Error("Task must be a function");

                // invokingEvent
                if (isBackground === false) {
                    var invokingEvent = new CustomEvent("MDPTaskInvoking", {
                        detail: {}
                    });
                    document.dispatchEvent(invokingEvent);
                }

                // worker
                const workerCode = `
                    onmessage = function(e) {
                        try {

                            // execute
                            var task = new Function('return ' + e.data.taskCode)();
                            var result = task(e.data.taskParameters);

                            // result
                            if (!(result instanceof Promise)) {
                                postMessage({ result });
                                return;
                            }

                            // promise
                            var promise = result;
                            promise.then((promiseResult) => {
                                postMessage({ result: promiseResult });
                            }).catch((err) => {
                                postMessage({ error: err.message });
                            });
                        } catch (error) {

                            // error
                            postMessage({ error: error.message });
                        }
                    }
                `;
                var worker = new Worker(URL.createObjectURL(new Blob([workerCode], { type: 'application/javascript' })));
                worker.onmessage = function (e) {

                    // result
                    var { result, error } = e.data;
                    if (!error) {
                        resolve(result);
                    } else {
                        reject(new Error(error));
                    }
                    worker.terminate();

                    // invokedEvent
                    if (isBackground === false) {
                        var invokedEvent = new CustomEvent("MDPTaskInvoked", {
                            detail: {}
                        });
                        document.dispatchEvent(invokedEvent);
                    }
                };
                worker.onerror = function (e) {

                    // reject
                    reject(new Error(e.message));
                    worker.terminate();

                    // invokedEvent
                    if (isBackground === false) {
                        var invokedEvent = new CustomEvent("MDPTaskInvoked", {
                            detail: {}
                        });
                        document.dispatchEvent(invokedEvent);
                    }
                };

                // invoke
                worker.postMessage({ taskCode: task.toString(), taskParameters: taskParameters });
            }
            catch (error) {

                // reject
                reject(error);
            }
        });
    }


    // return
    return {

        // methods
        invokeAsync: invokeAsync,
        invokeWorkerAsync: invokeWorkerAsync
    };
})();

document.addEventListener("MDPTaskInvoking", function (event) {

    // style
    document.body.classList.add("mdp-task-invoking");
});

document.addEventListener("MDPTaskInvoked", function (event) {

    // style
    document.body.classList.remove("mdp-task-invoking");
});

// mdp.blazorCore.errorManager
mdp.blazorCore.errorManager = (function () {

    // methods
    function dispatchError(error) {

        // require
        if (!error) return;
        if (typeof error !== 'object') error = new Error(error);

        // errorThrownEvent
        var errorThrownEvent = new CustomEvent("MDPErrorThrown", {
            detail: {
                error: error
            }
        });
        document.dispatchEvent(errorThrownEvent);

        // console
        console.error(error);
    }


    // return
    return {

        // methods
        dispatchError: dispatchError
    };
})();

// anchor.hotfix
document.addEventListener('click', function (event) {

    // require
    if (mdp.blazorCore.applicationManager.isInMaui() == false) return;

    // target='_blank'
    var anchor = event.target.closest('a[target="_blank"]');
    if (!anchor) return;

    // redirect
    event.preventDefault();
    window.location.href = anchor.href;
});


/* ---------- utilities ---------- */

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

// mdp.blazorCore.scrollManager
mdp.blazorCore.scrollManager = (function () {

    // fields
    var _scrollTop = 0;
    var _scrollState = null;
    var _scrollThreshold = 100;


    // methods
    function dispatchScrollUpped() {

        // style
        document.body.classList.remove("mdp-scroll-downed");

        // event
        document.body.dispatchEvent(new Event('scrollUpped'));
    }

    function dispatchScrollDowned() {

        // style
        document.body.classList.add("mdp-scroll-downed");

        // event
        document.body.dispatchEvent(new Event('scrollDowned'));
    }


    // handlers
    window.addEventListener("scroll", () => {

        // scrollTop
        var scrollTop = window.scrollY || window.scrollTop;
        if (!scrollTop) scrollTop = 0;
        if (Math.abs(_scrollTop - scrollTop) <= _scrollThreshold) return;

        // dispatch
        if (_scrollTop > scrollTop) {

            // scrollTop
            _scrollTop = scrollTop;

            // scrollState
            if (_scrollState == "scrollUpped") return;
            _scrollState = "scrollUpped";

            // dispatchScrollUpped
            dispatchScrollUpped();
        } else {

            // scrollTop
            _scrollTop = scrollTop;

            // scrollState
            if (_scrollState == "scrollDowned") return;
            _scrollState = "scrollDowned";

            // dispatchScrollDowned
            dispatchScrollDowned();
        }
    });


    // return
    return {

        // methods
        dispatchScrollUpped: dispatchScrollUpped,
        dispatchScrollDowned: dispatchScrollDowned
    };
})();

// mdp.blazorCore.transformManager
mdp.blazorCore.transformManager = (function () {

    // fields
    var _imageExtensionList = ['jpg', 'jpeg', 'png', 'gif', 'bmp'];


    // methods
    function transformImageAsync(file, outputWidth = 1080, outputHeight = 1080, outputFormat = "jpg", outputQuality = 0.85) {
        return new Promise((resolve, reject) => {
            try {

                // require
                if (!file) throw new Error("file is null");

                // fileExtension
                var fileExtension = file.name.split('.').pop().toLowerCase();
                if (_imageExtensionList.includes(fileExtension) == file) throw new Error(`Unsupported file type: ${fileExtension}`);

                // outputName
                var outputName = file.name.substring(0, file.name.lastIndexOf('.'));
                outputName = outputName + "." + outputFormat;

                // transform
                if (typeof OffscreenCanvas === "undefined") {
                    resolve(transformImageByCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName));
                } else {
                    resolve(transformImageByOffscreenCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName));
                }
            }
            catch (error) {

                // reject
                reject(error);
            }
        });
    }

    function transformImageByCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName) {
        return new Promise((resolve, reject) => {
            try {

                // fileReader
                var fileReader = new FileReader();
                fileReader.onload = function (event) {
                    try {

                        // image
                        var image = new Image();
                        image.onload = function () {
                            try {

                                // size
                                var width = image.width;
                                var height = image.height;
                                if (width > height) {
                                    height = (outputWidth / width) * height;
                                    width = outputWidth;
                                } else {
                                    width = (outputHeight / height) * width;
                                    height = outputHeight;
                                }

                                // canvas
                                var canvas = document.createElement('canvas');
                                var canvasContext = canvas.getContext('2d');
                                canvas.width = width;
                                canvas.height = height;
                                canvasContext.drawImage(image, 0, 0, width, height);

                                // imageBase64
                                canvas.toBlob(function (blob) {
                                    mdp.blazorCore.taskManager.invokeWorkerAsync((parameters) => {
                                        return new Promise((resolve, reject) => {
                                            try {

                                                // variables
                                                var blob = parameters.blob;
                                                var outputName = parameters.outputName;

                                                // fileReader
                                                const reader = new FileReader();
                                                reader.onloadend = function () {

                                                    // imageBase64
                                                    var imageBase64 = reader.result;
                                                    imageBase64 = imageBase64.split(',')[1];

                                                    // resolve
                                                    resolve({
                                                        "name": outputName,
                                                        "content": imageBase64
                                                    });
                                                };
                                                reader.onerror = reject;
                                                reader.readAsDataURL(blob);
                                            }
                                            catch (error) {

                                                // reject
                                                reject(error);
                                            }
                                        });
                                    }, { blob: blob, outputName: outputName }, true).then(resolve).catch(reject);
                                }, `image/${outputFormat}`, outputQuality);
                            }
                            catch (error) {

                                // reject
                                reject(error);
                            }
                        };
                        image.error = reject;
                        image.src = event.target.result;
                    }
                    catch (error) {

                        // reject
                        reject(error);
                    }
                };
                fileReader.error = reject;
                fileReader.readAsDataURL(file);
            } catch (error) {

                // reject
                reject(error);
            }
        });
    }

    function transformImageByOffscreenCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName) {
        return new Promise((resolve, reject) => {
            try {

                // fileReader
                var fileReader = new FileReader();
                fileReader.onload = function (event) {
                    try {

                        // image
                        var image = new Image();
                        image.onload = function () {
                            try {

                                // size
                                var width = image.width;
                                var height = image.height;
                                if (width > height) {
                                    height = (outputWidth / width) * height;
                                    width = outputWidth;
                                } else {
                                    width = (outputHeight / height) * width;
                                    height = outputHeight;
                                }

                                // canvas
                                var canvas = new OffscreenCanvas(width, height);
                                var canvasContext = canvas.getContext('2d');
                                canvasContext.drawImage(image, 0, 0, width, height);

                                // imageBase64
                                canvas.convertToBlob({ type: outputFormat, quality: outputQuality }).then(function (blob) {
                                    return mdp.blazorCore.taskManager.invokeWorkerAsync((parameters) => {
                                        return new Promise((resolve, reject) => {
                                            try {

                                                // variables
                                                var blob = parameters.blob;
                                                var outputName = parameters.outputName;

                                                // fileReader
                                                const reader = new FileReader();
                                                reader.onloadend = function () {

                                                    // imageBase64
                                                    var imageBase64 = reader.result;
                                                    imageBase64 = imageBase64.split(',')[1];

                                                    // resolve
                                                    resolve({
                                                        "name": outputName,
                                                        "content": imageBase64
                                                    });
                                                };
                                                reader.onerror = reject;
                                                reader.readAsDataURL(blob);
                                            }
                                            catch (error) {

                                                // reject
                                                reject(error);
                                            }
                                        });
                                    }, { blob: blob, outputName: outputName }, true);
                                }).then(resolve).catch(reject);
                            }
                            catch (error) {

                                // reject
                                reject(error);
                            }
                        };
                        image.onerror = reject;
                        image.src = event.target.result;
                    }
                    catch (error) {

                        // reject
                        reject(error);
                    }
                };
                fileReader.onerror = reject;
                fileReader.readAsDataURL(file);
            }
            catch (error) {

                // reject
                reject(error);
            }
        });
    }


    // return
    return {

        // methods
        transformImageAsync: transformImageAsync
    };
})();

// mdp.blazorCore.validationManager  
mdp.blazorCore.validationManager = (function () {

    // methods
    function isNumber(value) {

        // require
        if (value == null) return false;
        if (typeof value !== 'number') return false;

        // return
        return !isNaN(value);
    }

    function isNullOrEmpty(value) {

        // require
        if (value == null) return true;
        if (typeof value !== 'string') return true;

        // return
        return value.trim() === '';
    }

    function validateMail(value) {

        // require
        if (value == null) return false;
        if (typeof value !== 'string') return false;

        // variables
        const mailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        // return
        return mailRegex.test(value);
    }

    function validatePhone(value) {

        // require
        if (value == null) return false;
        if (typeof value !== 'string') return false;

        // variables
        const phoneRegex = /^(09)[0-9]{8}$/;

        // return
        return phoneRegex.test(value);
    }


    // return
    return {

        // methods
        isNumber: isNumber,
        isNullOrEmpty: isNullOrEmpty,
        validateMail: validateMail,
        validatePhone: validatePhone
    };
})();


/* ---------- components ---------- */

// mdp.blazorCore.fade
mdp.blazorCore.fade = function (fadeElement) {

    // require
    if (!fadeElement) return;
    if (fadeElement.fade) return fadeElement.fade;


    // methods
    function show(duration) {

        // show
        fadeElement.classList.add('show');
        if (!duration) return Promise.resolve();

        // auto-hide
        return new Promise((resolve, reject) => {
            setTimeout(function () {
                try {
                    fadeElement.fade.hide();
                    resolve();
                } catch (error) {
                    reject(error);
                }
            }, duration);
        });
    };

    function hide() {

        // hide
        fadeElement.classList.remove('show');
    };

    function toggle(duration) {
        if (fadeElement.classList.contains('fade-end') == true) {
            fadeElement.fade.show(duration);
        }
        else {
            fadeElement.fade.hide();
        }
    };


    // handlers
    fadeElement.addEventListener('transitionend', function () {

        // style
        if (window.getComputedStyle(fadeElement).getPropertyValue('opacity') == 0) {
            fadeElement.classList.add('fade-end');
        }
        else {
            fadeElement.classList.remove('fade-end');
        }
    });
    fadeElement.dispatchEvent(new Event('transitionend'));


    // return
    fadeElement.fade = {

        // methods
        show: show,
        hide: hide,
        toggle: toggle
    };
    return fadeElement.fade;
};

document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.fade:not([data-auto-start="false"]').forEach(function (fadeElement) {
        new mdp.blazorCore.fade(fadeElement);
    });
});

// mdp.blazorCore.popper
mdp.blazorCore.popper = function (popperElement) {

    // require
    if (!popperElement) return;
    if (popperElement.popper) return popperElement.popper;


    // fields
    var placement = popperElement.getAttribute('data-placement') || "bottom-start";

    // popper
    var popper = Popper.createPopper(document.body, popperElement, {
        placement: placement,
        modifiers: [
            {
                name: 'flip',
                options: {
                    fallbackPlacements: ['bottom', 'top', 'left', 'right']
                }
            },
            {
                name: 'preventOverflow',
                options: {
                    boundary: popperElement.parentElement
                }
            },
            {
                name: 'offset',
                options: {
                    offset: [0, 4],
                },
            },
        ],
    });


    // handlers
    document.addEventListener('click', function (event) {
        if (popperElement.classList.contains('popper-show') == true) {
            if (!popperElement.contains(event.target)) {
                popperElement.popper.hide();
            }
        }
    });


    // methods
    function show(hostElement) {

        // popper.update
        popper.state.elements.reference = hostElement; 
        popper.update();

        // show
        popperElement.classList.add('popper-show');
    };

    function hide() {

        // hide
        popperElement.classList.remove('popper-show');
    };

    function toggle(hostElement) {

        // display
        if (popperElement.classList.contains('popper-show') == false || popper.state.elements.reference != hostElement) {
            popperElement.popper.show(hostElement);
        }
        else {
            popperElement.popper.hide();
        }
    };

    function getHostElement() {
        return popper.state.elements.reference;
    }


    // return
    popperElement.popper = {

        // methods
        show: show,
        hide: hide,
        toggle: toggle,
        getHostElement: getHostElement
    };
    return popperElement.popper;
};

document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.popper:not([data-auto-start="false"]').forEach(function (popperElement) {
        new mdp.blazorCore.popper(popperElement);
    });
});

// mdp.blazorCore.offcanvas
document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.offcanvas').forEach(function (offcanvasElement) {

        // anchor.click
        offcanvasElement.querySelectorAll('a').forEach(function (anchorElement) {
            anchorElement.addEventListener('click', function (event) {
                
                // offcanvas.hide
                var offcanvas = bootstrap.Offcanvas.getInstance(offcanvasElement);
                if (offcanvas) offcanvas.hide();
            });
        });
    });
});

// textarea.autoHeight
document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('textarea').forEach(function (textareaElement) {

        // require
        if (!textareaElement) return;
        if (textareaElement.autoHeight) return;
        textareaElement.autoHeight = true;

        // handlers
        textareaElement.addEventListener('input', function () {
            textareaElement.style.height = 'auto';
            textareaElement.style.height = this.scrollHeight + 'px';
        });
        textareaElement.dispatchEvent(new Event('input'));
    });
});


/* ---------- swiper ---------- */

// mdp.blazorCore.swiperSlider
mdp.blazorCore.swiperSlider = function (swiperSliderElement) {

    // require
    if (!swiperSliderElement) return;
    if (swiperSliderElement.swiperSlider) return swiperSliderElement.swiperSlider;

    // fields
    var loop = swiperSliderElement.getAttribute('data-loop') == 'true' || false;
    var spaceBetween = parseInt(swiperSliderElement.getAttribute('data-space-between')) || 10;
    var pagination = swiperSliderElement.getAttribute('data-pagination') || ".swiper-pagination";
    var nextButton = swiperSliderElement.getAttribute('data-next-button') || ".swiper-button-next";
    var prevButton = swiperSliderElement.getAttribute('data-prev-button') || ".swiper-button-prev";
    var closeButton = swiperSliderElement.getAttribute('data-close-button') || ".swiper-button-close";

    // slideElement
    swiperSliderElement.querySelectorAll('.swiper-slide').forEach(slideElement => {

        // require
        var imageElement = slideElement.querySelector('img');
        if (!imageElement) return;

        // create
        var lazyPreloaderElement = document.createElement('div');
        lazyPreloaderElement.className = 'swiper-lazy-preloader swiper-lazy-preloader-white';
        slideElement.appendChild(lazyPreloaderElement);
    });

    // swiper
    var swiper = new Swiper(swiperSliderElement, {
        loop: loop,
        spaceBetween: spaceBetween,
        lazy: true,
        autoHeight: true,
        slidesPerView: 1,
        pagination: {
            el: pagination,
            clickable: true
        },
        navigation: {
            nextEl: nextButton,
            prevEl: prevButton,
        }
    });


    // handlers
    swiperSliderElement.querySelectorAll('.swiper-slide').forEach(slideElement => {
        slideElement.addEventListener('click', function () {

            // show
            swiperSliderElement.classList.toggle("swiper-fullscreen");
            document.body.classList.toggle("swiper-fullscreen");
        });
    });

    swiperSliderElement.querySelectorAll(closeButton).forEach(closeButtonElement => {
        closeButtonElement.addEventListener('click', function () {

            // hide
            swiperSliderElement.classList.toggle("swiper-fullscreen");
            document.body.classList.toggle("swiper-fullscreen");
        });
    });


    // return
    swiperSliderElement.swiperSlider = {

    };
    return swiperSliderElement.swiperSlider;
};

document.addEventListener('MDPPageLoaded', function () {
    document.querySelectorAll('.swiper-slider:not([data-auto-start="false"]').forEach(swiperSliderElement => {
        new mdp.blazorCore.swiperSlider(swiperSliderElement);
    });
});

// mdp.blazorCore.swiperWheel
mdp.blazorCore.swiperWheel = function (swiperWheelElement) {

    // require
    if (!swiperWheelElement) return;
    if (swiperWheelElement.swiperWheel) return swiperWheelElement.swiperWheel;


    // fields
    var slidesPerView = parseInt(swiperWheelElement.getAttribute('data-slides-perview')) || 5;

    // swiperWheelElement
    swiperWheelElement.style.height = (parseFloat(window.getComputedStyle(swiperWheelElement).lineHeight) * slidesPerView) + "px";

    // maskContainer
    var maskContainer = document.createElement('div');
    maskContainer.className = 'swiper-mask-container';
    swiperWheelElement.appendChild(maskContainer);

    // topMaskElement
    var topMaskElement = document.createElement('div');
    topMaskElement.className = 'swiper-mask-top';
    maskContainer.appendChild(topMaskElement);

    // middleMaskElement
    var middleMaskElement = document.createElement('div');
    middleMaskElement.className = 'swiper-mask-middle';
    maskContainer.appendChild(middleMaskElement);

    // bottomMaskElement
    var bottomMaskElement = document.createElement('div');
    bottomMaskElement.className = 'swiper-mask-bottom';
    maskContainer.appendChild(bottomMaskElement);
    
    // swiper
    var swiper = new Swiper(swiperWheelElement, {
        direction: "vertical",
        slidesPerView: slidesPerView,
        mousewheel: true,
        spaceBetween: 0,
        centeredSlides: true,
        slideToClickedSlide: false,
        on: {
            slideChange: function () {

                // slideChange
                var slideChangeEvent = new CustomEvent('slideChange', {
                    detail: {
                    }
                });
                swiperWheelElement.dispatchEvent(slideChangeEvent);
            },
            click: function (event) {

                // slideTo
                swiperWheelElement.swiperWheel.slideTo(o => o == event.clickedSlide);
            }
        }
    });


    // methods
    function update() {

        // update
        swiper.update();
    }

    function slideTo(predicate) {

        // slideTo
        var activeIndex = 0;
        for (let i = 0; i < swiper.slides.length; i++) {
            if (window.getComputedStyle(swiper.slides[i]).display !== "none") {
                if (predicate(swiper.slides[i]) == true) {
                    swiper.slideTo(activeIndex);
                    return;
                }
                activeIndex++;
            }
        }
    }

    function slideToLast() {

        // slideTo
        swiper.slideTo(swiper.slides.length);
    }
    
    function slideToFirst() {

        // slideTo
        swiper.slideTo(0);
    }

    function getActiveIndex() {

        // require
        if (swiper.slides.length <= 0) return -1;

        // return
        return swiper.activeIndex;
    }

    function getActiveSlide() {

        // require
        if (swiper.slides.length <= 0) return null;

        // activeSlide
        var activeSlide = Array.from(swiper.slides).filter(o => window.getComputedStyle(o).display !== "none")[swiper.activeIndex] || null;
        if (!activeSlide) activeSlide = null;

        // return
        return activeSlide;
    }
    

    // return
    swiperWheelElement.swiperWheel = {
        update: update,
        slideTo: slideTo,
        slideToLast: slideToLast,
        slideToFirst: slideToFirst,
        getActiveIndex: getActiveIndex,
        getActiveSlide: getActiveSlide
    };
    return swiperWheelElement.swiperWheel;
};

document.addEventListener('MDPPageLoaded', function () {
    document.querySelectorAll('.swiper-wheel:not([data-auto-start="false"]').forEach(swiperWheelElement => {
        new mdp.blazorCore.swiperWheel(swiperWheelElement);
    });
});


/* ---------- picker ---------- */

// mdp.blazorCore.filePicker
mdp.blazorCore.filePicker = function (filePickerElement) {

    // require
    if (!filePickerElement) return;
    if (filePickerElement.filePicker) return filePickerElement.filePicker;


    // fields
    var _imageExtensionList = ['jpg', 'jpeg', 'png', 'gif', 'bmp'];
    var _src = filePickerElement.getAttribute('data-src') || "";
    var _accept = filePickerElement.getAttribute('data-accept') || "";

    // previewFileElement
    var previewFileElement = document.createElement("div");
    previewFileElement.className = "preview-file";
    filePickerElement.appendChild(previewFileElement);

    // previewImageElement
    var previewImageElement = document.createElement("img");
    previewImageElement.className = "preview-image";
    filePickerElement.appendChild(previewImageElement);

    // fileInputElement
    var fileInputElement = document.createElement("input");
    fileInputElement.type = "file";
    fileInputElement.className = "file-input";
    fileInputElement.accept = _accept;
    filePickerElement.appendChild(fileInputElement);


    // methods
    function clear() {

        // clear
        filePickerElement.classList.remove("file-selected");
        previewImageElement.src = "";
        previewImageElement.style.opacity = "0";
        previewFileElement.textContent = "";
        previewFileElement.style.opacity = "0";
        fileInputElement.value = "";
    }

    function reset() {

        // clear
        filePickerElement.classList.remove("file-selected");
        previewImageElement.src = "";
        previewImageElement.style.opacity = "0";
        previewFileElement.textContent = "";
        previewFileElement.style.opacity = "0";
        fileInputElement.value = "";

        // fileName
        var fileName = null;
        if (_src) {
            fileName = new URL(_src, window.location.origin).pathname.split('/').pop();
        }
        if (!fileName || fileName.trim() === '') return;

        // fileExtension
        var fileExtension = null;
        if (fileName.includes('.') == true) fileExtension = fileName.split('.').pop().toLowerCase();

        // src.type
        if (!fileExtension || _imageExtensionList.includes(fileExtension) == true) {

            // filePickerElement
            filePickerElement.classList.add("file-selected");

            // previewImageElement
            previewImageElement.src = _src;
            previewImageElement.style.opacity = "1";
        } else {

            // filePickerElement
            filePickerElement.classList.add("file-selected");

            // previewFileElement
            previewFileElement.textContent = fileName;
            previewFileElement.style.opacity = "1";
        }
    }

    function hasFile() {

        // require
        if (fileInputElement.files.length <= 0) return false;

        // return
        return true;
    }

    function getFile() {

        // require
        if (fileInputElement.files.length <= 0) return null;

        // return
        return fileInputElement.files[0];
    }

    function hasImage() {

        // require
        if (fileInputElement.files.length <= 0) return false;

        // fileExtension
        var fileExtension = fileInputElement.files[0].name.split('.').pop().toLowerCase();
        if (_imageExtensionList.includes(fileExtension) == false) return false;

        // return
        return true;
    }

    function getImage() {

        // require
        if (hasImage() == false) return null;

        // return
        return fileInputElement.files[0];
    }


    // handlers
    previewImageElement.addEventListener('error', function () {

        // errorImage
        var errorSrc = filePickerElement.getAttribute('data-src-error');
        if (errorSrc) previewImageElement.src = errorSrc;
    });

    fileInputElement.addEventListener('change', function (event) {

        // require
        var file = event.target.files[0];
        if (!file) return;

        // clear
        filePickerElement.classList.remove("file-selected");
        previewImageElement.src = "";
        previewImageElement.style.opacity = "0";
        previewFileElement.textContent = "";
        previewFileElement.style.opacity = "0";

        // file.type
        if (file.type.startsWith("image/") == true) {

            // filePickerElement
            filePickerElement.classList.add("file-selected");

            // previewImageElement
            var fileReader = new FileReader();
            fileReader.onload = function (e) {
                previewImageElement.src = e.target.result;
                previewImageElement.style.opacity = "1";
            };
            fileReader.readAsDataURL(file);
        } else {

            // filePickerElement
            filePickerElement.classList.add("file-selected");

            // previewFileElement
            previewFileElement.textContent = file.name;
            previewFileElement.style.opacity = "1";
        }
    });


    // constructors
    (function () {

        // reset
        reset();
    })();


    // return
    filePickerElement.filePicker = {

        // methods
        clear: clear,
        reset: reset,
        hasFile: hasFile,
        getFile: getFile,
        hasImage: hasImage,
        getImage: getImage
    };
    return filePickerElement.filePicker;
};

document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.mdp-wrapper .file-picker:not([data-auto-start="false"]').forEach(function (filePickerElement) {
        new mdp.blazorCore.filePicker(filePickerElement);
    });
});

// mdp.blazorCore.timePicker
mdp.blazorCore.timePicker = function (timePickerElement) {

    // require
    if (!timePickerElement) return;
    if (timePickerElement.timePicker) return timePickerElement.timePicker;

    // fields
    var _minTime = moment("00:00", "HH:mm");
    var _maxTime = moment("24:00", "HH:mm");
    var _perviewCount = parseInt(timePickerElement.getAttribute('data-perview-count')) || 5;
    var _stepDuration = Number(timePickerElement.getAttribute('data-step-duration')) || 30;

    // hourWheelElement
    var hourWheelElement = document.createElement("div");
    hourWheelElement.className = "time-picker-hours swiper-wheel swiper";
    hourWheelElement.setAttribute('data-slides-perview', _perviewCount);
    timePickerElement.appendChild(hourWheelElement);
    
    var hourWrapperElement = document.createElement("div");
    hourWrapperElement.className = "swiper-wrapper";
    hourWheelElement.appendChild(hourWrapperElement);

    var indexHour = 0;
    while (indexHour <= 24) {
        var slideElement = document.createElement("div");
        slideElement.className = "swiper-slide";
        slideElement.textContent = indexHour.toString().padStart(2, '0');
        hourWrapperElement.appendChild(slideElement);
        indexHour = indexHour + 1;
    }
    new mdp.blazorCore.swiperWheel(hourWheelElement);

    // colonElement
    var colonElement = document.createElement("div");
    colonElement.className = 'time-picker-colon';
    colonElement.innerHTML = ":";
    timePickerElement.appendChild(colonElement);

    // minuteWheelElement
    var minuteWheelElement = document.createElement("div");
    minuteWheelElement.className = "time-picker-minutes swiper-wheel swiper";
    minuteWheelElement.setAttribute('data-slides-perview', _perviewCount);
    timePickerElement.appendChild(minuteWheelElement);

    var minuteWrapperElement = document.createElement("div");
    minuteWrapperElement.className = "swiper-wrapper";
    minuteWheelElement.appendChild(minuteWrapperElement);

    var indexMinute = 0;
    while (indexMinute < 60) {
        var slideElement = document.createElement("div");
        slideElement.className = "swiper-slide";
        slideElement.textContent = indexMinute.toString().padStart(2, '0');
        minuteWrapperElement.appendChild(slideElement);
        indexMinute = indexMinute + _stepDuration;
    }
    new mdp.blazorCore.swiperWheel(minuteWheelElement);

    // maskContainer
    var maskContainer = document.createElement('div');
    maskContainer.className = 'time-picker-mask-container';
    timePickerElement.appendChild(maskContainer);

    // middleMaskElement
    var middleMaskElement = document.createElement('div');
    middleMaskElement.className = 'time-picker-mask-middle';
    maskContainer.appendChild(middleMaskElement);


    // methods
    function getTime() {

        // variables
        var hourString = hourWheelElement.swiperWheel.getActiveSlide()?.textContent;
        var minuteString = minuteWheelElement.swiperWheel.getActiveSlide()?.textContent;

        // return
        if (!hourString) return null;
        if (!minuteString) return null;
        return moment(hourString + ":" + minuteString, 'HH:mm');
    }

    function setTime(time) {

        // require
        if (moment.isMoment(time) == false) throw new Error('time 必須為 moment 物件');
        if (time.isSameOrBefore(moment("00:00", "HH:mm"))) time = moment("00:00", "HH:mm");
        if (time.isSameOrAfter(moment("24:00", "HH:mm"))) time = moment("24:00", "HH:mm");

        // timeString
        var hourString = null;
        var minuteString = null;
        if (time.isSame(moment("24:00", "HH:mm"))) {
            hourString = "24";
            minuteString = "00";
        }
        else {
            hourString = time.hour().toString().padStart(2, '0');
            minuteString = time.minute().toString().padStart(2, '0');
        }

        // slideTo
        hourWheelElement.swiperWheel.update();
        hourWheelElement.swiperWheel.slideTo(o => o.textContent == hourString);
        minuteWheelElement.swiperWheel.update();
        minuteWheelElement.swiperWheel.slideTo(o => o.textContent == minuteString);
    }

    function setTimeRange(minTime, maxTime) {
        
        // require
        if (moment.isMoment(minTime) == false) throw new Error('minTime 必須為 moment 物件');
        if (moment.isMoment(maxTime) == false) throw new Error('maxTime 必須為 moment 物件');
        if (minTime.isSameOrBefore(moment("00:00", "HH:mm"))) minTime = moment("00:00", "HH:mm");
        if (maxTime.isSameOrAfter(moment("24:00", "HH:mm"))) maxTime = moment("24:00", "HH:mm");
        if (maxTime.isBefore(minTime)) maxTime = minTime.clone();

        // variables
        _minTime = minTime;
        _maxTime = maxTime;
        var activeIndex = hourWheelElement.swiperWheel.getActiveIndex();

        // refresh
        hourWheelElement.querySelectorAll(".swiper-slide").forEach(slideElement => {
            var slideTime = moment(slideElement.textContent.trim(), 'HH').startOf('hour');
            if (slideTime.isSameOrAfter(_minTime.clone().startOf('hour')) && slideTime.isSameOrBefore(_maxTime)) {
                slideElement.classList.remove('d-none');
            } else {
                slideElement.classList.add('d-none');                
            }
        });
        hourWheelElement.swiper.update();

        // slideChange
        if (hourWheelElement.swiperWheel.getActiveIndex() == activeIndex) {
            var slideChangeEvent = new CustomEvent('slideChange', {
                detail: {

                }
            });
            hourWheelElement.dispatchEvent(slideChangeEvent);
        }
    }

    function getStepDuration() {

        // return
        return _stepDuration;
    }


    // handlers
    hourWheelElement.addEventListener('slideChange', function () {

        // variables
        var hourString = hourWheelElement.swiperWheel.getActiveSlide()?.textContent || "00";
        var activeIndex = minuteWheelElement.swiperWheel.getActiveIndex();

        // refresh
        minuteWheelElement.querySelectorAll(".swiper-slide").forEach(slideElement => {
            var slideTime = moment(hourString + ":" + slideElement.textContent, 'HH:mm');
            if (slideTime.isSameOrAfter(_minTime) && slideTime.isSameOrBefore(_maxTime)) {
                slideElement.classList.remove('d-none');
            } else {
                slideElement.classList.add('d-none');
            }
        });
        minuteWheelElement.swiper.update();

        // timeChange
        if (minuteWheelElement.swiperWheel.getActiveIndex() == activeIndex) {
            var timeChangeEvent = new CustomEvent('timeChange', {
                detail: {

                }
            });
            timePickerElement.dispatchEvent(timeChangeEvent);
        }
    });

    minuteWheelElement.addEventListener('slideChange', function () {

        // timeChange
        var timeChangeEvent = new CustomEvent('timeChange', {
            detail: {

            }
        });
        timePickerElement.dispatchEvent(timeChangeEvent);
    });


    // return
    timePickerElement.timePicker = {
        getTime: getTime,
        getStepDuration: getStepDuration,
        setTime: setTime,
        setTimeRange: setTimeRange
    };
    return timePickerElement.timePicker;
};

document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.mdp-wrapper .time-picker:not([data-auto-start="false"]').forEach(function (timePickerElement) {
        new mdp.blazorCore.timePicker(timePickerElement);
    });
});