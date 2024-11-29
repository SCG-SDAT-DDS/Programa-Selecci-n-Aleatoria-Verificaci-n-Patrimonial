//!function ($) {

//    "use strict"; // jshint ;_;

//    /* TYPEAHEAD PUBLIC CLASS DEFINITION
//     * ================================= */

//    var Typeahead = function (element, options) {

//        //deal with scrollBar
//        var defaultOptions = $.fn.typeahead.defaults;
//        if (options.scrollBar) {
//            options.items = 100;
//            options.menu = '<ul class="typeahead dropdown-menu" style="max-height:220px;overflow:auto;"></ul>';
//        }

//        var that = this;
//        that.$element = $(element);
//        that.options = $.extend({}, $.fn.typeahead.defaults, options);
//        that.$menu = $(that.options.menu).insertAfter(that.$element);

//        // Method overrides
//        that.eventSupported = that.options.eventSupported || that.eventSupported;
//        that.grepper = that.options.grepper || that.grepper;
//        that.highlighter = that.options.highlighter || that.highlighter;
//        that.lookup = that.options.lookup || that.lookup;
//        that.matcher = that.options.matcher || that.matcher;
//        that.render = that.options.render || that.render;
//        that.onSelect = that.options.onSelect || null;
//        that.sorter = that.options.sorter || that.sorter;
//        that.source = that.options.source || that.source;
//        that.displayField = that.options.displayField || that.displayField;
//        that.valueField = that.options.valueField || that.valueField;
//        that.autoSelect = that.options.autoSelect || that.autoSelect;

//        if (that.options.ajax) {
//            var ajax = that.options.ajax;

//            if (typeof ajax === 'string') {
//                that.ajax = $.extend({}, $.fn.typeahead.defaults.ajax, {
//                    url: ajax
//                });
//            } else {
//                if (typeof ajax.displayField === 'string') {
//                    that.displayField = that.options.displayField = ajax.displayField;
//                }
//                if (typeof ajax.valueField === 'string') {
//                    that.valueField = that.options.valueField = ajax.valueField;
//                }

//                that.ajax = $.extend({}, $.fn.typeahead.defaults.ajax, ajax);
//            }

//            if (!that.ajax.url) {
//                that.ajax = null;
//            }
//            that.query = "";
//        } else {
//            that.source = that.options.source;
//            that.ajax = null;
//        }
//        that.shown = false;
//        that.listen();
//    };

//    Typeahead.prototype = {
//        constructor: Typeahead,
//        //=============================================================================================================
//        //  Utils
//        //  Check if an event is supported by the browser eg. 'keypress'
//        //  * This was included to handle the "exhaustive deprecation" of jQuery.browser in jQuery 1.8
//        //=============================================================================================================
//        eventSupported: function (eventName) {
//            var isSupported = (eventName in this.$element);

//            if (!isSupported) {
//                this.$element.setAttribute(eventName, 'return;');
//                isSupported = typeof this.$element[eventName] === 'function';
//            }

//            return isSupported;
//        },
//        select: function () {
//            var $selectedItem = this.$menu.find('.active');
//            if($selectedItem.length) {
//                var value = $selectedItem.attr('data-value');
//                var text = this.$menu.find('.active a').text();

//                if (this.options.onSelect) {
//                    this.options.onSelect({
//                        value: value,
//                        text: text
//                    });
//                }
//                this.$element
//                    .val(this.updater(text))
//                    .change();
//            }
//            return this.hide();
//        },
//        updater: function (item) {
//            return item;
//        },
//        show: function () {
//            var pos = $.extend({}, this.$element.position(), {
//                height: this.$element[0].offsetHeight
//            });

//            this.$menu.css({
//                top: pos.top + pos.height,
//                left: pos.left
//            });

//            if(this.options.alignWidth) {
//                var width = $(this.$element[0]).outerWidth();
//                this.$menu.css({
//                    width: width
//                });
//            }

//            this.$menu.show();
//            this.shown = true;
//            return this;
//        },
//        hide: function () {
//            this.$menu.hide();
//            this.shown = false;
//            return this;
//        },
//        ajaxLookup: function () {

//            var query = $.trim(this.$element.val());

//            if (query === this.query) {
//                return this;
//            }

//            // Query changed
//            this.query = query;

//            // Cancel last timer if set
//            if (this.ajax.timerId) {
//                clearTimeout(this.ajax.timerId);
//                this.ajax.timerId = null;
//            }

//            if (!query || query.length < this.ajax.triggerLength) {
//                // cancel the ajax callback if in progress
//                if (this.ajax.xhr) {
//                    this.ajax.xhr.abort();
//                    this.ajax.xhr = null;
//                    this.ajaxToggleLoadClass(false);
//                }

//                return this.shown ? this.hide() : this;
//            }

//            function execute() {
//                this.ajaxToggleLoadClass(true);

//                // Cancel last call if already in progress
//                if (this.ajax.xhr)
//                    this.ajax.xhr.abort();

//                var params = this.ajax.preDispatch ? this.ajax.preDispatch(query) : {
//                    query: query
//                };
//                this.ajax.xhr = $.ajax({
//                    url: this.ajax.url,
//                    data: params,
//                    success: $.proxy(this.ajaxSource, this),
//                    type: this.ajax.method || 'get',
//                    dataType: 'json',
//                    headers: this.ajax.headers || {}
//                });
//                this.ajax.timerId = null;
//            }

//            // Query is good to send, set a timer
//            this.ajax.timerId = setTimeout($.proxy(execute, this), this.ajax.timeout);

//            return this;
//        },
//        ajaxSource: function (data) {
//            this.ajaxToggleLoadClass(false);
//            var that = this, items;
//            if (!that.ajax.xhr)
//                return;
//            if (that.ajax.preProcess) {
//                data = that.ajax.preProcess(data);
//            }
//            // Save for selection retreival
//            that.ajax.data = data;

//            // Manipulate objects
//            items = that.grepper(that.ajax.data) || [];
//            if (!items.length) {
//                return that.shown ? that.hide() : that;
//            }

//            that.ajax.xhr = null;
//            return that.render(items.slice(0, that.options.items)).show();
//        },
//        ajaxToggleLoadClass: function (enable) {
//            if (!this.ajax.loadingClass)
//                return;
//            this.$element.toggleClass(this.ajax.loadingClass, enable);
//        },
//        lookup: function (event) {
//            var that = this, items;
//            if (that.ajax) {
//                that.ajaxer();
//            }
//            else {
//                that.query = that.$element.val();

//                if (!that.query) {
//                    return that.shown ? that.hide() : that;
//                }

//                items = that.grepper(that.source);


//                if (!items) {
//                    return that.shown ? that.hide() : that;
//                }
//                //Bhanu added a custom message- Result not Found when no result is found
//                if (items.length == 0) {
//                    items[0] = {'id': -21, 'name': "Result not Found"}
//                }
//                return that.render(items.slice(0, that.options.items)).show();
//            }
//        },
//        matcher: function (item) {
//            return ~item.toLowerCase().indexOf(this.query.toLowerCase());
//        },
//        sorter: function (items) {
//            if (!this.options.ajax) {
//                var beginswith = [],
//                    caseSensitive = [],
//                    caseInsensitive = [],
//                    item;

//                while (item = items.shift()) {
//                    if (!item.toLowerCase().indexOf(this.query.toLowerCase()))
//                        beginswith.push(item);
//                    else if (~item.indexOf(this.query))
//                        caseSensitive.push(item);
//                    else
//                        caseInsensitive.push(item);
//                }

//                return beginswith.concat(caseSensitive, caseInsensitive);
//            } else {
//                return items;
//            }
//        },
//        highlighter: function (item) {
//            var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
//            return item.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
//                return '<strong>' + match + '</strong>';
//            });
//        },
//        render: function (items) {
//            var that = this, display, isString = typeof that.options.displayField === 'string';

//            items = $(items).map(function (i, item) {
//                if (typeof item === 'object') {
//                    display = isString ? item[that.options.displayField] : that.options.displayField(item);
//                    i = $(that.options.item).attr('data-value', item[that.options.valueField]);
//                } else {
//                    display = item;
//                    i = $(that.options.item).attr('data-value', item);
//                }
//                i.find('a').html(that.highlighter(display));
//                return i[0];
//            });

//            if(that.autoSelect){
//                items.first().addClass('active');
//            }

//            this.$menu.html(items);
//            return this;
//        },
//        //------------------------------------------------------------------
//        //  Filters relevent results
//        //
//        grepper: function (data) {
//            var that = this, items, display, isString = typeof that.options.displayField === 'string';

//            if (isString && data && data.length) {
//                if (data[0].hasOwnProperty(that.options.displayField)) {
//                    items = $.grep(data, function (item) {
//                        display = isString ? item[that.options.displayField] : that.options.displayField(item);
//                        return that.matcher(display);
//                    });
//                } else if (typeof data[0] === 'string') {
//                    items = $.grep(data, function (item) {
//                        return that.matcher(item);
//                    });
//                } else {
//                    return null;
//                }
//            } else {
//                return null;
//            }
//            return this.sorter(items);
//        },
//        next: function (event) {
//            var active = this.$menu.find('.active').removeClass('active'),
//                next = active.next();

//            if (!next.length) {
//                next = $(this.$menu.find('li')[0]);
//            }

//            if (this.options.scrollBar) {
//                var index = this.$menu.children("li").index(next);
//                if (index % 8 == 0) {
//                    this.$menu.scrollTop(index * 26);
//                }
//            }

//            next.addClass('active');
//        },
//        prev: function (event) {
//            var active = this.$menu.find('.active').removeClass('active'),
//                prev = active.prev();

//            if (!prev.length) {
//                prev = this.$menu.find('li').last();
//            }

//            if (this.options.scrollBar) {

//                var $li = this.$menu.children("li");
//                var total = $li.length - 1;
//                var index = $li.index(prev);

//                if ((total - index) % 8 == 0) {
//                    this.$menu.scrollTop((index - 7) * 26);
//                }

//            }

//            prev.addClass('active');

//        },
//        listen: function () {
//            this.$element
//                .on('focus', $.proxy(this.focus, this))
//                .on('blur', $.proxy(this.blur, this))
//                .on('keypress', $.proxy(this.keypress, this))
//                .on('keyup', $.proxy(this.keyup, this));

//            if (this.eventSupported('keydown')) {
//                this.$element.on('keydown', $.proxy(this.keydown, this))
//            }

//            this.$menu
//                .on('click', $.proxy(this.click, this))
//                .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
//                .on('mouseleave', 'li', $.proxy(this.mouseleave, this))
//        },
//        move: function (e) {
//            if (!this.shown)
//                return

//            switch (e.keyCode) {
//                case 9: // tab
//                case 13: // enter
//                case 27: // escape
//                    e.preventDefault();
//                    break

//                case 38: // up arrow
//                    e.preventDefault()
//                    this.prev()
//                    break

//                case 40: // down arrow
//                    e.preventDefault()
//                    this.next()
//                    break
//            }

//            e.stopPropagation();
//        },
//        keydown: function (e) {
//            this.suppressKeyPressRepeat = ~$.inArray(e.keyCode, [40, 38, 9, 13, 27])
//            this.move(e)
//        },
//        keypress: function (e) {
//            if (this.suppressKeyPressRepeat)
//                return
//            this.move(e)
//        },
//        keyup: function (e) {
//            switch (e.keyCode) {
//                case 40: // down arrow
//                case 38: // up arrow
//                case 16: // shift
//                case 17: // ctrl
//                case 18: // alt
//                    break

//                case 9: // tab
//                case 13: // enter
//                    if (!this.shown)
//                        return
//                    this.select()
//                    break

//                case 27: // escape
//                    if (!this.shown)
//                        return
//                    this.hide()
//                    break

//                default:
//                    if (this.ajax)
//                        this.ajaxLookup()
//                    else
//                        this.lookup()
//            }

//            e.stopPropagation()
//            e.preventDefault()
//        },
//        focus: function (e) {
//            this.focused = true
//        },
//        blur: function (e) {
//            this.focused = false
//            if (!this.mousedover && this.shown)
//                this.hide()
//        },
//        click: function (e) {
//            e.stopPropagation()
//            e.preventDefault()
//            this.select()
//            this.$element.focus()
//        },
//        mouseenter: function (e) {
//            this.mousedover = true
//            this.$menu.find('.active').removeClass('active')
//            $(e.currentTarget).addClass('active')
//        },
//        mouseleave: function (e) {
//            this.mousedover = false
//            if (!this.focused && this.shown)
//                this.hide()
//        },
//        destroy: function() {
//            this.$element
//                .off('focus', $.proxy(this.focus, this))
//                .off('blur', $.proxy(this.blur, this))
//                .off('keypress', $.proxy(this.keypress, this))
//                .off('keyup', $.proxy(this.keyup, this));

//            if (this.eventSupported('keydown')) {
//                this.$element.off('keydown', $.proxy(this.keydown, this))
//            }

//            this.$menu
//                .off('click', $.proxy(this.click, this))
//                .off('mouseenter', 'li', $.proxy(this.mouseenter, this))
//                .off('mouseleave', 'li', $.proxy(this.mouseleave, this))
//            this.$element.removeData('typeahead');
//        }
//    };


//    /* TYPEAHEAD PLUGIN DEFINITION
//     * =========================== */

//    $.fn.typeahead = function (option) {
//        return this.each(function () {
//            var $this = $(this),
//                data = $this.data('typeahead'),
//                options = typeof option === 'object' && option;
//            if (!data)
//                $this.data('typeahead', (data = new Typeahead(this, options)));
//            if (typeof option === 'string')
//                data[option]();
//        });
//    };

//    $.fn.typeahead.defaults = {
//        source: [],
//        items: 10,
//        scrollBar: false,
//        alignWidth: true,
//        menu: '<ul class="typeahead dropdown-menu"></ul>',
//        item: '<li><a href="#"></a></li>',
//        valueField: 'id',
//        displayField: 'name',
//        autoSelect: true,
//        onSelect: function () {
//        },
//        ajax: {
//            url: null,
//            timeout: 300,
//            method: 'get',
//            triggerLength: 1,
//            loadingClass: null,
//            preDispatch: null,
//            preProcess: null
//        }
//    };

//    $.fn.typeahead.Constructor = Typeahead;

//    /* TYPEAHEAD DATA-API
//     * ================== */

//    $(function () {
//        $('body').on('focus.typeahead.data-api', '[data-provide="typeahead"]', function (e) {
//            var $this = $(this);
//            if ($this.data('typeahead'))
//                return;
//            e.preventDefault();
//            $this.typeahead($this.data());
//        });
//    });

//}(window.jQuery);














/* =============================================================
 * bootstrap3-typeahead.js v4.0.2
 * https://github.com/bassjobsen/Bootstrap-3-Typeahead
 * =============================================================
 * Original written by @mdo and @fat
 * =============================================================
 * Copyright 2014 Bass Jobsen @bassjobsen
 *
 * Licensed under the Apache License, Version 2.0 (the 'License');
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an 'AS IS' BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ============================================================ */


(function (root, factory) {

    'use strict';

    // CommonJS module is defined
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = factory(require('jquery'));
    }
        // AMD module is defined
    else if (typeof define === 'function' && define.amd) {
        define(['jquery'], function ($) {
            return factory($);
        });
    } else {
        factory(root.jQuery);
    }

}(this, function ($) {

    'use strict';
    // jshint laxcomma: true


    /* TYPEAHEAD PUBLIC CLASS DEFINITION
     * ================================= */

    var Typeahead = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, Typeahead.defaults, options);
        this.matcher = this.options.matcher || this.matcher;
        this.sorter = this.options.sorter || this.sorter;
        this.select = this.options.select || this.select;
        this.autoSelect = typeof this.options.autoSelect == 'boolean' ? this.options.autoSelect : true;
        this.highlighter = this.options.highlighter || this.highlighter;
        this.render = this.options.render || this.render;
        this.updater = this.options.updater || this.updater;
        this.displayText = this.options.displayText || this.displayText;
        this.source = this.options.source;
        this.delay = this.options.delay;
        this.$menu = $(this.options.menu);
        this.$appendTo = this.options.appendTo ? $(this.options.appendTo) : null;
        this.fitToElement = typeof this.options.fitToElement == 'boolean' ? this.options.fitToElement : false;
        this.shown = false;
        this.listen();
        this.showHintOnFocus = typeof this.options.showHintOnFocus == 'boolean' || this.options.showHintOnFocus === "all" ? this.options.showHintOnFocus : false;
        this.afterSelect = this.options.afterSelect;
        this.addItem = false;
        this.value = this.$element.val() || this.$element.text();
    };

    Typeahead.prototype = {

        constructor: Typeahead,

        select: function () {
            var val = this.$menu.find('.active').data('value');
            this.$element.data('active', val);
            if (this.autoSelect || val) {
                var newVal = this.updater(val);
                // Updater can be set to any random functions via "options" parameter in constructor above.
                // Add null check for cases when updater returns void or undefined.
                if (!newVal) {
                    newVal = '';
                }
                this.$element
                  .val(this.displayText(newVal) || newVal)
                  .text(this.displayText(newVal) || newVal)
                  .change();
                this.afterSelect(newVal);
            }
            return this.hide();
        },

        updater: function (item) {
            return item;
        },

        setSource: function (source) {
            this.source = source;
        },

        show: function () {
            var pos = $.extend({}, this.$element.position(), {
                height: this.$element[0].offsetHeight
            });

            var scrollHeight = typeof this.options.scrollHeight == 'function' ?
                this.options.scrollHeight.call() :
                this.options.scrollHeight;

            var element;
            if (this.shown) {
                element = this.$menu;
            } else if (this.$appendTo) {
                element = this.$menu.appendTo(this.$appendTo);
                this.hasSameParent = this.$appendTo.is(this.$element.parent());
            } else {
                element = this.$menu.insertAfter(this.$element);
                this.hasSameParent = true;
            }

            if (!this.hasSameParent) {
                // We cannot rely on the element position, need to position relative to the window
                element.css("position", "fixed");
                var offset = this.$element.offset();
                pos.top = offset.top;
                pos.left = offset.left;
            }
            // The rules for bootstrap are: 'dropup' in the parent and 'dropdown-menu-right' in the element.
            // Note that to get right alignment, you'll need to specify `menu` in the options to be:
            // '<ul class="typeahead dropdown-menu" role="listbox"></ul>'
            var dropup = $(element).parent().hasClass('dropup');
            var newTop = dropup ? 'auto' : (pos.top + pos.height + scrollHeight);
            var right = $(element).hasClass('dropdown-menu-right');
            var newLeft = right ? 'auto' : pos.left;
            // it seems like setting the css is a bad idea (just let Bootstrap do it), but I'll keep the old
            // logic in place except for the dropup/right-align cases.
            element.css({ top: newTop, left: newLeft }).show();

            if (this.options.fitToElement === true) {
                element.css("width", this.$element.outerWidth() + "px");
            }

            this.shown = true;
            return this;
        },

        hide: function () {
            this.$menu.hide();
            this.shown = false;
            return this;
        },

        lookup: function (query) {
            var items;
            if (typeof (query) != 'undefined' && query !== null) {
                this.query = query;
            } else {
                this.query = this.$element.val() || this.$element.text() || '';
            }

            if (this.query.length < this.options.minLength && !this.options.showHintOnFocus) {
                return this.shown ? this.hide() : this;
            }

            var worker = $.proxy(function () {

                if ($.isFunction(this.source)) {
                    this.source(this.query, $.proxy(this.process, this));
                } else if (this.source) {
                    this.process(this.source);
                }
            }, this);

            clearTimeout(this.lookupWorker);
            this.lookupWorker = setTimeout(worker, this.delay);
        },

        process: function (items) {
            var that = this;

            items = $.grep(items, function (item) {
                return that.matcher(item);
            });

            items = this.sorter(items);

            if (!items.length && !this.options.addItem) {
                return this.shown ? this.hide() : this;
            }

            if (items.length > 0) {
                this.$element.data('active', items[0]);
            } else {
                this.$element.data('active', null);
            }

            if (this.options.items != 'all') {
                items = items.slice(0, this.options.items);
            }

            // Add item
            if (this.options.addItem) {
                items.push(this.options.addItem);
            }

            return this.render(items).show();
        },

        matcher: function (item) {
            var it = this.displayText(item);
            return ~it.toLowerCase().indexOf(this.query.toLowerCase());
        },

        sorter: function (items) {
            var beginswith = [];
            var caseSensitive = [];
            var caseInsensitive = [];
            var item;

            while ((item = items.shift())) {
                var it = this.displayText(item);
                if (!it.toLowerCase().indexOf(this.query.toLowerCase())) beginswith.push(item);
                else if (~it.indexOf(this.query)) caseSensitive.push(item);
                else caseInsensitive.push(item);
            }

            return beginswith.concat(caseSensitive, caseInsensitive);
        },

        highlighter: function (item) {
            var html = $('<div></div>');
            var query = this.query;
            var i = item.toLowerCase().indexOf(query.toLowerCase());
            var len = query.length;
            var leftPart;
            var middlePart;
            var rightPart;
            var strong;
            if (len === 0) {
                return html.text(item).html();
            }
            while (i > -1) {
                leftPart = item.substr(0, i);
                middlePart = item.substr(i, len);
                rightPart = item.substr(i + len);
                strong = $('<strong></strong>').text(middlePart);
                html
                  .append(document.createTextNode(leftPart))
                  .append(strong);
                item = rightPart;
                i = item.toLowerCase().indexOf(query.toLowerCase());
            }
            return html.append(document.createTextNode(item)).html();
        },

        render: function (items) {
            var that = this;
            var self = this;
            var activeFound = false;
            var data = [];
            var _category = that.options.separator;

            $.each(items, function (key, value) {
                // inject separator
                if (key > 0 && value[_category] !== items[key - 1][_category]) {
                    data.push({
                        __type: 'divider'
                    });
                }

                // inject category header
                if (value[_category] && (key === 0 || value[_category] !== items[key - 1][_category])) {
                    data.push({
                        __type: 'category',
                        name: value[_category]
                    });
                }
                data.push(value);
            });

            items = $(data).map(function (i, item) {
                if ((item.__type || false) == 'category') {
                    return $(that.options.headerHtml).text(item.name)[0];
                }

                if ((item.__type || false) == 'divider') {
                    return $(that.options.headerDivider)[0];
                }

                var text = self.displayText(item);
                i = $(that.options.item).data('value', item);
                i.find('a').html(that.highlighter(text, item));
                if (text == self.$element.val()) {
                    i.addClass('active');
                    self.$element.data('active', item);
                    activeFound = true;
                }
                return i[0];
            });

            if (this.autoSelect && !activeFound) {
                items.filter(':not(.dropdown-header)').first().addClass('active');
                this.$element.data('active', items.first().data('value'));
            }
            this.$menu.html(items);
            return this;
        },

        displayText: function (item) {
            return typeof item !== 'undefined' && typeof item.name != 'undefined' ? item.name : item;
        },

        next: function (event) {
            var active = this.$menu.find('.active').removeClass('active');
            var next = active.next();

            if (!next.length) {
                next = $(this.$menu.find('li')[0]);
            }

            next.addClass('active');
        },

        prev: function (event) {
            var active = this.$menu.find('.active').removeClass('active');
            var prev = active.prev();

            if (!prev.length) {
                prev = this.$menu.find('li').last();
            }

            prev.addClass('active');
        },

        listen: function () {
            this.$element
              .on('focus', $.proxy(this.focus, this))
              .on('blur', $.proxy(this.blur, this))
              .on('keypress', $.proxy(this.keypress, this))
              .on('propertychange input', $.proxy(this.input, this))
              .on('keyup', $.proxy(this.keyup, this));

            if (this.eventSupported('keydown')) {
                this.$element.on('keydown', $.proxy(this.keydown, this));
            }

            this.$menu
              .on('click', $.proxy(this.click, this))
              .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
              .on('mouseleave', 'li', $.proxy(this.mouseleave, this))
              .on('mousedown', $.proxy(this.mousedown, this));
        },

        destroy: function () {
            this.$element.data('typeahead', null);
            this.$element.data('active', null);
            this.$element
              .off('focus')
              .off('blur')
              .off('keypress')
              .off('propertychange input')
              .off('keyup');

            if (this.eventSupported('keydown')) {
                this.$element.off('keydown');
            }

            this.$menu.remove();
            this.destroyed = true;
        },

        eventSupported: function (eventName) {
            var isSupported = eventName in this.$element;
            if (!isSupported) {
                this.$element.setAttribute(eventName, 'return;');
                isSupported = typeof this.$element[eventName] === 'function';
            }
            return isSupported;
        },

        move: function (e) {
            if (!this.shown) return;

            switch (e.keyCode) {
                case 9: // tab
                case 13: // enter
                case 27: // escape
                    e.preventDefault();
                    break;

                case 38: // up arrow
                    // with the shiftKey (this is actually the left parenthesis)
                    if (e.shiftKey) return;
                    e.preventDefault();
                    this.prev();
                    break;

                case 40: // down arrow
                    // with the shiftKey (this is actually the right parenthesis)
                    if (e.shiftKey) return;
                    e.preventDefault();
                    this.next();
                    break;
            }
        },

        keydown: function (e) {
            this.suppressKeyPressRepeat = ~$.inArray(e.keyCode, [40, 38, 9, 13, 27]);
            if (!this.shown && e.keyCode == 40) {
                this.lookup();
            } else {
                this.move(e);
            }
        },

        keypress: function (e) {
            if (this.suppressKeyPressRepeat) return;
            this.move(e);
        },

        input: function (e) {
            // This is a fixed for IE10/11 that fires the input event when a placehoder is changed
            // (https://connect.microsoft.com/IE/feedback/details/810538/ie-11-fires-input-event-on-focus)
            var currentValue = this.$element.val() || this.$element.text();
            if (this.value !== currentValue) {
                this.value = currentValue;
                this.lookup();
            }
        },

        keyup: function (e) {
            if (this.destroyed) {
                return;
            }
            switch (e.keyCode) {
                case 40: // down arrow
                case 38: // up arrow
                case 16: // shift
                case 17: // ctrl
                case 18: // alt
                    break;

                case 9: // tab
                case 13: // enter
                    if (!this.shown) return;
                    this.select();
                    break;

                case 27: // escape
                    if (!this.shown) return;
                    this.hide();
                    break;
            }


        },

        focus: function (e) {
            if (!this.focused) {
                this.focused = true;
                if (this.options.showHintOnFocus && this.skipShowHintOnFocus !== true) {
                    if (this.options.showHintOnFocus === "all") {
                        this.lookup("");
                    } else {
                        this.lookup();
                    }
                }
            }
            if (this.skipShowHintOnFocus) {
                this.skipShowHintOnFocus = false;
            }
        },

        blur: function (e) {
            if (!this.mousedover && !this.mouseddown && this.shown) {
                this.hide();
                this.focused = false;
            } else if (this.mouseddown) {
                // This is for IE that blurs the input when user clicks on scroll.
                // We set the focus back on the input and prevent the lookup to occur again
                this.skipShowHintOnFocus = true;
                this.$element.focus();
                this.mouseddown = false;
            }
        },

        click: function (e) {
            e.preventDefault();
            this.skipShowHintOnFocus = true;
            this.select();
            this.$element.focus();
            this.hide();
        },

        mouseenter: function (e) {
            this.mousedover = true;
            this.$menu.find('.active').removeClass('active');
            $(e.currentTarget).addClass('active');
        },

        mouseleave: function (e) {
            this.mousedover = false;
            if (!this.focused && this.shown) this.hide();
        },

        /**
          * We track the mousedown for IE. When clicking on the menu scrollbar, IE makes the input blur thus hiding the menu.
          */
        mousedown: function (e) {
            this.mouseddown = true;
            this.$menu.one("mouseup", function (e) {
                // IE won't fire this, but FF and Chrome will so we reset our flag for them here
                this.mouseddown = false;
            }.bind(this));
        }

    };


    /* TYPEAHEAD PLUGIN DEFINITION
     * =========================== */

    var old = $.fn.typeahead;

    $.fn.typeahead = function (option) {
        var arg = arguments;
        if (typeof option == 'string' && option == 'getActive') {
            return this.data('active');
        }
        return this.each(function () {
            var $this = $(this);
            var data = $this.data('typeahead');
            var options = typeof option == 'object' && option;
            if (!data) $this.data('typeahead', (data = new Typeahead(this, options)));
            if (typeof option == 'string' && data[option]) {
                if (arg.length > 1) {
                    data[option].apply(data, Array.prototype.slice.call(arg, 1));
                } else {
                    data[option]();
                }
            }
        });
    };

    Typeahead.defaults = {
        source: [],
        items: 8,
        menu: '<ul class="typeahead dropdown-menu" role="listbox"></ul>',
        item: '<li><a class="dropdown-item" href="#" role="option"></a></li>',
        minLength: 1,
        scrollHeight: 0,
        autoSelect: true,
        afterSelect: $.noop,
        addItem: false,
        delay: 0,
        separator: 'category',
        headerHtml: '<li class="dropdown-header"></li>',
        headerDivider: '<li class="divider" role="separator"></li>'
    };

    $.fn.typeahead.Constructor = Typeahead;

    /* TYPEAHEAD NO CONFLICT
     * =================== */

    $.fn.typeahead.noConflict = function () {
        $.fn.typeahead = old;
        return this;
    };


    /* TYPEAHEAD DATA-API
     * ================== */

    $(document).on('focus.typeahead.data-api', '[data-provide="typeahead"]', function (e) {
        var $this = $(this);
        if ($this.data('typeahead')) return;
        $this.typeahead($this.data());
    });

}));