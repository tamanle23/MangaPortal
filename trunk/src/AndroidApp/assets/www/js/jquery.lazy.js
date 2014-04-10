(function ($, window, document, undefined) {
    $.fn.lazy = function (settings) {
        var self = this;
        self.isInitializing = false;
        var configuration =
        {
            bind: "load",
            threshold: 200,
            fallbackHeight: 2000,
            visibleOnly: true,
            firstLoadArea: 200,
            delay: -1,
            combined: false,
            attribute: "data-src",
            removeAttribute: true,
            effect: "show",
            effectTime: 0,
            enableThrottle: false,
            throttle: 250,
            beforeLoad: null,
            onLoad: null,
            afterLoad: null,
            onError: null
        };
        if (settings)
            $.extend(configuration, settings);
        var items = this;
        // on first page load get initial images
        if (configuration.bind == "load")
            $(window).load(initialize);
            // if event driven don't wait for page loading
        else if (configuration.bind == "event")
            initialize();
        // bind error callback to images if wanted
        if (configuration.onError)
            items.bind("error", function () {
                configuration.onError($(this));
            });
        function lazyLoadImages(allImages) {
            if (typeof allImages != "boolean")
                allImages = false;
            
            items.each(function () {
                    var element = $(this);
                    var minWidth=$(element).parent().width();
                    var tag = this.tagName.toLowerCase();

                    if (isInLoadableArea(element) || allImages)
                    {
                        
                        if (element.attr(configuration.attribute)
                            &&
                            // and is image tag where attribute is not
                            // equal source
                            ((tag == "img" && element
                                .attr(configuration.attribute) != element
                                .attr("src")) ||
                            // or is non image tag where attribute is
                            // not equal background
                            ((tag != "img" && element
                                .attr(configuration.attribute) != element
                                .css("background-image"))))
                            &&
                            // and is not actually loaded just before
                            !element.data("loaded")
                            &&
                            // and is visible or visibility doesn't
                            // matter
                            (element.is(":visible") || !configuration.visibleOnly))
                        {
                            // create image object
                            var imageObj = $(new Image());
                            // copy element information to pseudo image
                            // because we return this element in "onLoad"
                            // and "onError"
                            $.each(this.attributes, function (index, attr) {
                                if (attr.name != "src") {
                                    // i know, there is a shorter way to do
                                    // the following
                                    // but this is the best workaround for
                                    // ie6/7
                                    var value = element.attr(attr.name);
                                    imageObj.attr(attr.name, value);
                                }
                            });
                            // bind error event if wanted
                            if (configuration.onError)
                                imageObj.error(function () {
                                    configuration.onError(imageObj);
                                });
                            // bind after load callback to image
                            var onLoad = true;
                            imageObj
                                .one(
                                    "load",
                                    function () {

                                        var callable = function () {
                                            if (onLoad) {

                                                window.setTimeout(
                                                    callable,
                                                    100);
                                                return;
                                            }
                                            // remove element from
                                            // view
                                            element.hide();
                                            
                                            // set image back to
                                            // element
                                            if (tag == "img")
                                                element
                                                    .attr(
                                                        "src",
                                                        imageObj
                                                            .attr("src"));
                                            else
                                                element
                                                    .css(
                                                        "background-image",
                                                        "url("
                                                            + imageObj
                                                                .attr("src")
                                                            + ")");
                                            // bring it back with
                                            // some effect!
                                            element[configuration.effect]
                                                (configuration.effectTime);
                                            // remove attribute from
                                            // element
                                            if (configuration.removeAttribute)
                                                element
                                                    .removeAttr(configuration.attribute);
                                            // call after load event
                                            if (configuration.afterLoad)
                                                configuration
                                                    .afterLoad(element);
                                            // unbind event and
                                            // remove image object
                                            imageObj.unbind("load");
                                            imageObj.remove();
                                           
                                            element.css("width", minWidth);
                                           
                                        };
                                        callable();
                                    });
                            // trigger function before loading image
                            if (configuration.beforeLoad)
                                configuration.beforeLoad(element);
                            // set source
                            imageObj.attr("src", element
                                .attr(configuration.attribute));
                            // trigger function before loading image
                            if (configuration.onLoad)
                                configuration.onLoad(imageObj);
                            onLoad = false;
                            // call after load even on cached image
                            if (imageObj.complete)
                                imageObj.load();
                            // mark element always as loaded
                            element.data("loaded", true);
                            
                        }
                       
                    }
                });
            // cleanup all items which are already loaded
            items = $(items).filter(function () {
                return !$(this).data("loaded");
            });
        }
        function initialize() {
            self.isInitializing = true;
            // if delay time is set load all images at once after delay time
            if (configuration.delay >= 0)
                setTimeout(function () {
                    lazyLoadImages(true);
                }, configuration.delay);
            // if no delay is set or combine usage is active bind events
            if (configuration.delay < 0 || configuration.combined) {
                // load initial images
                lazyLoadImages(false);
                var host = $(items).parents(".contenthost").toArray()[0];
                if (typeof (host) == "undefined") {
                    // bind lazy load functions to scroll and resize event
                    $(window).on("scroll",
                        throttle(configuration.throttle, lazyLoadImages));
                    $(window).on("resize",
                        throttle(configuration.throttle, lazyLoadImages));
                } else {
                    // bind lazy load functions to scroll and resize event
                    $(host).on("scroll",
                        throttle(configuration.throttle, lazyLoadImages));
                    $(host).on("resize",
                        throttle(configuration.throttle, lazyLoadImages));
                }
            }
            self.isInitializing = false;
        }
        function isInLoadableArea(element) {
            var top = document.documentElement.scrollTop ? document.documentElement.scrollTop
                : document.body.scrollTop;
            var loadArea = 0;
            if (self.isInitializing) {
                loadArea = (top + configuration.firstLoadArea);
            } else
                loadArea = (top + getActualHeight() + configuration.threshold);
            var bot = (element.offset().top + element.height());
            return loadArea > bot;
        }
        function getActualHeight() {
            if (self.isInitializing)
                return configuration.firstLoadArea;
            if (window.innerHeight)
                return window.innerHeight;
            if (document.documentElement
                && document.documentElement.clientHeight)
                return document.documentElement.clientHeight;
            if (document.body && document.body.clientHeight)
                return document.body.clientHeight;
            if (document.body && document.body.offsetHeight)
                return document.body.offsetHeight;
            return configuration.fallbackHeight;
        }
        function throttle(delay, call) {
            var _timeout;
            var _exec = 0;
            function callable() {
                var elapsed = +new Date() - _exec;
                function run() {
                    _exec = +new Date();
                    call.apply();
                }
                _timeout && clearTimeout(_timeout);
                if (elapsed > delay || !configuration.enableThrottle)
                    run();
                else
                    _timeout = setTimeout(run, delay - elapsed);
            }
            return callable;
        }
        return this;
    };
    $.fn.Lazy = $.fn.lazy;
})(jQuery, window, document);