/**
 * DevExtreme (integration/jquery/easing.js)
 * Version: 18.2.8
 * Build date: Tue Apr 23 2019
 *
 * Copyright (c) 2012 - 2019 Developer Express Inc. ALL RIGHTS RESERVED
 * Read about DevExtreme licensing here: https://js.devexpress.com/Licensing/
 */
"use strict";
var jQuery = require("jquery"),
    easing = require("../../animation/easing");
if (jQuery) {
    easing.setEasing(jQuery.easing)
}
