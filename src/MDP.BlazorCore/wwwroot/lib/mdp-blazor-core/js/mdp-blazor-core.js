// mdp
var mdp = mdp || {};


// mdp.blazorCore
mdp.blazorCore = mdp.blazorCore || {};

// mdp.blazorCore.pageManager
mdp.blazorCore.pageManager = (function () {

    // events
    function onPageLoading() {

        // pageLoading
        var pageLoadingEvent = new CustomEvent("MDPPageLoading", {
            detail: {}
        });
        document.dispatchEvent(pageLoadingEvent);
    }

    function onPageLoaded() {

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

        // events
        onPageLoading: onPageLoading,
        onPageLoaded: onPageLoaded
    };
})();

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

// mdp.blazorCore.errorManager
mdp.blazorCore.errorManager = (function () {

    // events
    function onError(error) {

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

        // events
        onError: onError
    };
})();

// mdp.blazorCore.transformManager
mdp.blazorCore.transformManager = (function () {

    // const
    const _imageExtensionList = ['jpg', 'jpeg', 'png', 'gif', 'bmp'];


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
                //if (typeof OffscreenCanvas === "undefined") {
                //    resolve(transformImageByCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName));
                //} else {
                //    resolve(transformImageByOffscreenCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName));
                //}
                resolve(transformImageByCanvasAsync(file, outputWidth, outputHeight, outputFormat, outputQuality, outputName));
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
        transformImageAsync: transformImageAsync
    };
})();


/* ---------- native ---------- */
(function () {

    // textarea
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

    // error
    Error.prototype.toJSON = function () {
        return {
            name: this.name,
            message: this.message
        };
    };
})();


/* ---------- bootstrap ---------- */
(function () {

    // fade
    document.addEventListener("MDPPageLoaded", function (event) {
        document.querySelectorAll('.fade').forEach(function (fadeElement) {

            // require
            if (!fadeElement) return;
            if (fadeElement.fade) return;

            // fade
            var fade = (function () {

                // methods
                function show(duration) {

                    // duration
                    if (!duration && fadeElement.getAttribute('data-show-duration')) {
                        duration = parseInt(fadeElement.getAttribute('data-show-duration'), 10);
                    }

                    // show
                    fadeElement.classList.add('show');

                    // auto-hide
                    if (duration) {
                        setTimeout(function () {
                            fadeElement.classList.remove('show');
                        }, duration);
                    };
                };

                function hide() {
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


                // return
                return {

                    // methods
                    show: show,
                    hide: hide,
                    toggle: toggle
                };
            })();
            fadeElement.fade = fade;

            // handlers
            fadeElement.addEventListener('transitionend', function () {
                if (window.getComputedStyle(fadeElement).getPropertyValue('opacity') == 0) {
                    fadeElement.classList.add('fade-end');
                }
                else {
                    fadeElement.classList.remove('fade-end');
                }
            });
            fadeElement.dispatchEvent(new Event('transitionend'));
        });
    });
})();


/* ---------- mdp-blazor-core ---------- */
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

        // mdp.blazorCore.errorManager
        if (event.detail.pageError) {
            mdp.blazorCore.errorManager.onError(new Error(event.detail.pageError.message));
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

    // taskInvoking
    document.addEventListener("MDPTaskInvoking", function (event) {

        // style
        document.body.classList.add("mdp-task-invoking");
    });

    // taskInvoked
    document.addEventListener("MDPTaskInvoked", function (event) {

        // style
        document.body.classList.remove("mdp-task-invoking");
    });


    // scrollMonitor
    document.addEventListener("MDPPageLoaded", function (event) {
        document.querySelectorAll('.scroll-monitor').forEach(function (scrollMonitorElement) {
            
            // require
            if (!scrollMonitorElement) return;
            if (scrollMonitorElement.scrollMonitor) return;
            if (scrollMonitorElement === document.body) scrollMonitorElement = window;
            if (scrollMonitorElement === document.documentElement) scrollMonitorElement = window;
            
            // scrollMonitor
            var scrollMonitor = (function () {

                // fields
                var _scrollTop = 0;
                var _scrollState = null;
                var _scrollThreshold = 100;


                // handlers
                scrollMonitorElement.addEventListener("scroll", () => {

                    // scrollTop
                    var scrollTop = scrollMonitorElement.scrollY || scrollMonitorElement.scrollTop;
                    if (!scrollTop) scrollTop = 0;
                    if (Math.abs(_scrollTop - scrollTop) <= _scrollThreshold) return;

                    // dispatch
                    if (_scrollTop > scrollTop) {

                        // scrollTop
                        _scrollTop = scrollTop;

                        // scrollState
                        if (_scrollState == "scrollUpped") return;
                        _scrollState = "scrollUpped";

                        // onScrollUpped
                        onScrollUpped();
                    } else {

                        // scrollTop
                        _scrollTop = scrollTop;

                        // scrollState
                        if (_scrollState == "scrollDowned") return;
                        _scrollState = "scrollDowned";

                        // onScrollDowned
                        onScrollDowned();
                    }
                });


                // events
                function onScrollUpped() {

                    // style
                    if (scrollMonitorElement === window) {
                        document.body.classList.remove("mdp-scroll-downed");
                    }
                    else {
                        scrollMonitorElement.classList.remove("mdp-scroll-downed");
                    }
                }

                function onScrollDowned() {

                    // style
                    if (scrollMonitorElement === window) {
                        document.body.classList.add("mdp-scroll-downed");
                    }
                    else {
                        scrollMonitorElement.classList.add("mdp-scroll-downed");
                    }
                }


                // return
                return {

                    // events
                    onScrollUpped: onScrollUpped,
                    onScrollDowned: onScrollDowned
                };
            })();
            scrollMonitorElement.scrollMonitor = scrollMonitor;
            if (scrollMonitorElement === window) document.body.scrollMonitor = scrollMonitor;
        });
    });

    // filePicker
    document.addEventListener("MDPPageLoaded", function (event) {
        document.querySelectorAll('.file-picker').forEach(function (filePickerElement) {

            // require
            if (!filePickerElement) return;
            if (filePickerElement.filePicker) return;

            // variables
            var accept = filePickerElement.getAttribute('data-accept') || "";

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
            fileInputElement.accept = accept;
            filePickerElement.appendChild(fileInputElement);

            // filePicker
            var filePicker = (function () {

                // const
                const _imageExtensionList = ['jpg', 'jpeg', 'png', 'gif', 'bmp'];


                // fields
                var _src = filePickerElement.getAttribute('data-src') || "";


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
                        fileName = new URL(_src).pathname.split('/').pop();
                    }
                    if (!fileName) return;
                    if (fileName.includes('.') == false) return;
                    if (fileName.startsWith('.') == true) return;

                    // fileExtension
                    var fileExtension = fileName.split('.').pop().toLowerCase();
                    if (!fileExtension) return;

                    // src.type
                    if (_imageExtensionList.includes(fileExtension) == true) {

                        // filePickerElement
                        filePickerElement.classList.add("file-selected");

                        // previewImageElement
                        previewImageElement.src = _src;
                        previewImageElement.style.opacity = "1";
                    } else {

                        // filePickerElement
                        filePickerElement.classList.add("file-selected");

                        // previewFileElement
                        previewFileElement.textContent = file.name;
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
                return {
                    clear: clear,
                    reset: reset,
                    hasFile: hasFile,
                    getFile: getFile,
                    hasImage: hasImage,
                    getImage: getImage
                };
            })();
            filePickerElement.filePicker = filePicker;
        });
    });
})();


/* ---------- swiper ---------- */
(function () {

    // swiperSlider
    document.addEventListener('MDPPageLoaded', function () {
        document.querySelectorAll('.mdp-wrapper .swiper-slider').forEach(swiperSliderElement => {

            // require
            if (!swiperSliderElement) return;
            if (swiperSliderElement.swiperSlider) return;

            // variables
            var loop = swiperSliderElement.getAttribute('data-loop') == 'true' || false;
            var spaceBetween = parseInt(swiperSliderElement.getAttribute('data-space-between')) || 10;
            var pagination = swiperSliderElement.getAttribute('data-pagination') || ".swiper-pagination";
            var nextButton = swiperSliderElement.getAttribute('data-next-button') || ".swiper-button-next";
            var prevButton = swiperSliderElement.getAttribute('data-prev-button') || ".swiper-button-prev";
            var closeButton = swiperSliderElement.getAttribute('data-close-button') || ".swiper-button-close";
            
            // lazyPreloaderElement
            swiperSliderElement.querySelectorAll('.swiper-slide').forEach(slideElement => {

                // require
                var imageElement = slideElement.querySelector('img');
                if (!imageElement) return;

                // create
                var lazyPreloaderElement = document.createElement('div');
                lazyPreloaderElement.className = 'swiper-lazy-preloader swiper-lazy-preloader-white';
                slideElement.appendChild(lazyPreloaderElement);
            });

            // swiperSlider
            var swiperSlider = new Swiper(swiperSliderElement, {
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
            swiperSliderElement.swiperSlider = swiperSlider;

            // swiperSlider.handlers
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
        });
    });

    // swiperWheel
    document.addEventListener('MDPPageLoaded', function () {
        document.querySelectorAll('.mdp-wrapper .swiper-wheel').forEach(swiperWheelElement => {

            // require
            if (!swiperWheelElement) return;
            if (swiperWheelElement.swiperWheel) return;

            // variables
            var slidesPerView = parseInt(swiperWheelElement.getAttribute('data-slides-perview')) || 5;

            // topMaskElement
            var topMaskElement = document.createElement('div');
            topMaskElement.className = 'swiper-mask-top';
            swiperWheelElement.appendChild(topMaskElement);

            // bottomMaskElement
            var bottomMaskElement = document.createElement('div');
            bottomMaskElement.className = 'swiper-mask-bottom';
            swiperWheelElement.appendChild(bottomMaskElement);

            // swiperWheel
            var swiperWheel = new Swiper(swiperWheelElement, {
                direction: "vertical",
                slidesPerView: slidesPerView,
                mousewheel: true,
                spaceBetween: 0,
                centeredSlides: true,
                slideToClickedSlide: true
            });
            swiperWheelElement.swiperWheel = swiperWheel;

            // maskHeight
            var slideHeight = swiperWheel.slides[swiperWheel.activeIndex].offsetHeight;
            var spaceHeight = swiperWheel.params.spaceBetween;
            var maskSlideCount = Math.floor(slidesPerView / 2);
            var maskHeight = (slideHeight + spaceHeight) * maskSlideCount;
            topMaskElement.style.height = maskHeight + "px";
            bottomMaskElement.style.height = maskHeight + "px";
        });
    });    
})();