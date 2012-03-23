q = function() { }
q.declared = function(something) { return !(something === undefined); }
q.defined = function(something) { return q.declared(something) && something != null; }
q.tag = function(name, className) { var result = $(document.createElement(name)); if (q.defined(className)) { result.addClass(className); } return result; }
q.div = function(className) { return q.tag("DIV", className); }
q.span = function(className) { return q.tag("SPAN", className); }
q.empty = function(container) { for (var node = container.firstNode; node != null; node = node.nextSibling) alert(node.nodeType); }
q.required = function(something, name) { if (!q.defined(something)) throw new Error(name + " is not defined."); }

q.icons = { plus: 0, up: 1, down: 2, remove: 3, right: 4, left: 5, last: 6, first: 7, thumbup: 8, thumbdown: 9 };
q.icon = function(key, title, handler) {
    return q.icon(key, title, handler, "");
}

q.icon = function(key, title, handler, id) {
    var result = q.tag("img", "icon").attr("src", vpath + "Images/t.gif");
    if (q.defined(handler)) {
        result.click(function(e) {
            result.reset();
            handler(e);
        });
    }
    if (id != "") {
        result.attr("id", id);
    }

    var index = q.icons[key] || 0;
    var x = "-" + (index * 21) + "px";
    if (q.defined(title)) {
        result.attr("title", title);
    }
    result.reset = function() { result.css("background-position", x + " bottom"); }
    result.hover(function() { result.css("background-position", x + " bottom"); }, result.reset);
    result.reset();
    return result;
}
q.sicon = function(key, title) {
	var result = q.div("icon");
	var index = q.icons[key] || 0;
	var x = "-" + (index * 21) + "px";
	if (q.defined(title)) {
		result.attr("title", title);
	}
	result.css("background-position", x + " bottom");
	return result;
}


q.Tabs = function() { }
q.Tabs.switchTabs = function(element, tabId, url) {
	if (q.defined(url)) {
		window.location.href = url;
	} else {
		$(".tabs .active").removeClass("active");
		var tab = $(element);
		tab.addClass("active");
		$("div.tab-content").hide();
		if (q.defined(tabId)) {
			$("#" + tabId).show();
		}
	}
}
