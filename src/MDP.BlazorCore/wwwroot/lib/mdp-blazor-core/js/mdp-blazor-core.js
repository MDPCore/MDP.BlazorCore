/* ---------- global ---------- */

// error.toJSON
Error.prototype.toJSON = function () {
    return {
        name: this.name,
        message: this.message
    };
};

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


/* ---------- platform ---------- */

// mdp
window.mdp = window.mdp || {};

// mdp.blazorCore
mdp.blazorCore = mdp.blazorCore || {};

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

    // scroll
    window.scrollTo({ top: 0, behavior: 'auto' });
});

document.addEventListener("MDPPageLoaded", function (event) {

    // style
    document.body.classList.remove("mdp-page-loading");

    // scroll
    window.scrollTo({ top: 0, behavior: 'auto' });

    // mdp.blazorCore.errorManager
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


/* ---------- utilities ---------- */

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


/* ---------- components ---------- */

// mdp.blazorCore.Fade
mdp.blazorCore.Fade = function (fadeElement) {

    // require
    if (!fadeElement) return;
    if (fadeElement.fade) return fadeElement.fade;


    // methods
    function show(duration) {

        // duration
        if (!duration && fadeElement.getAttribute('data-show-duration')) {
            duration = parseInt(fadeElement.getAttribute('data-show-duration'), 10);
        }

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
        new mdp.blazorCore.Fade(fadeElement);
    });
});

// mdp.blazorCore.ScrollMonitor
mdp.blazorCore.ScrollMonitor = function (scrollMonitorElement) {

    // require
    if (!scrollMonitorElement) return;
    if (scrollMonitorElement.scrollMonitor) return scrollMonitorElement.scrollMonitor;
    if (scrollMonitorElement === document.body) scrollMonitorElement = window;
    if (scrollMonitorElement === document.documentElement) scrollMonitorElement = window;

    // fields
    var _scrollTop = 0;
    var _scrollState = null;
    var _scrollThreshold = 100;
    

    // methods
    function dispatchScrollUpped() {

        // style
        if (scrollMonitorElement === window) {
            document.body.classList.remove("mdp-scroll-downed");
        }
        else {
            scrollMonitorElement.classList.remove("mdp-scroll-downed");
        }

        // event
        if (scrollMonitorElement === window) {
            document.body.dispatchEvent(new Event('scrollUpped'));
        }
        else {
            scrollMonitorElement.dispatchEvent(new Event('scrollUpped'));
        }
    }

    function dispatchScrollDowned() {

        // style
        if (scrollMonitorElement === window) {
            document.body.classList.add("mdp-scroll-downed");
        }
        else {
            scrollMonitorElement.classList.add("mdp-scroll-downed");
        }

        // event
        if (scrollMonitorElement === window) {
            document.body.dispatchEvent(new Event('scrollDowned'));
        }
        else {
            scrollMonitorElement.dispatchEvent(new Event('scrollDowned'));
        }
    }


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
    scrollMonitorElement.scrollMonitor = {

        // methods
        dispatchScrollUpped: dispatchScrollUpped,
        dispatchScrollDowned: dispatchScrollDowned
    };
    return scrollMonitorElement.scrollMonitor;
};

document.addEventListener("MDPPageLoaded", function (event) {
    document.querySelectorAll('.scroll-monitor:not([data-auto-start="false"]').forEach(function (scrollMonitorElement) {
        new mdp.blazorCore.ScrollMonitor(scrollMonitorElement);
    });
});


/* ---------- picker ---------- */

// mdp.blazorCore.FilePicker
mdp.blazorCore.FilePicker = function (filePickerElement) {

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
    document.querySelectorAll('.file-picker:not([data-auto-start="false"]').forEach(function (filePickerElement) {
        new mdp.blazorCore.FilePicker(filePickerElement);
    });
});


/* ---------- swiper ---------- */

// mdp.blazorCore.SwiperSlider
mdp.blazorCore.SwiperSlider = function (swiperSliderElement) {

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
    document.querySelectorAll('.mdp-wrapper .swiper-slider:not([data-auto-start="false"]').forEach(swiperSliderElement => {
        new mdp.blazorCore.SwiperSlider(swiperSliderElement);
    });
});

// mdp.blazorCore.SwiperWheel
mdp.blazorCore.SwiperWheel = function (swiperWheelElement) {

    // require
    if (!swiperWheelElement) return;
    if (swiperWheelElement.swiperWheel) return swiperWheelElement.swiperWheel;

    // fields
    var slidesPerView = parseInt(swiperWheelElement.getAttribute('data-slides-perview')) || 5;

    // topMaskElement
    var topMaskElement = document.createElement('div');
    topMaskElement.className = 'swiper-mask-top';
    swiperWheelElement.appendChild(topMaskElement);

    // bottomMaskElement
    var bottomMaskElement = document.createElement('div');
    bottomMaskElement.className = 'swiper-mask-bottom';
    swiperWheelElement.appendChild(bottomMaskElement);

    // swiper
    var swiper = new Swiper(swiperWheelElement, {
        direction: "vertical",
        slidesPerView: slidesPerView,
        mousewheel: true,
        spaceBetween: 0,
        centeredSlides: true,
        slideToClickedSlide: true
    });

    // maskHeight
    var slideHeight = swiper.slides[swiper.activeIndex].offsetHeight;
    var spaceHeight = swiper.params.spaceBetween;
    var maskSlideCount = Math.floor(slidesPerView / 2);
    var maskHeight = (slideHeight + spaceHeight) * maskSlideCount;
    topMaskElement.style.height = maskHeight + "px";
    bottomMaskElement.style.height = maskHeight + "px";

    // return
    swiperSliderElement.swiperWheel = {

    };
    return swiperSliderElement.swiperWheel;
};

document.addEventListener('MDPPageLoaded', function () {
    document.querySelectorAll('.mdp-wrapper .swiper-wheel:not([data-auto-start="false"]').forEach(swiperWheelElement => {
        new mdp.blazorCore.SwiperWheel(swiperWheelElement);
    });
});