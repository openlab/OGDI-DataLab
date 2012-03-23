Ogdi = function() { };
Ogdi.QueryBuilder = function(container, meta, target, targetContainer) {
	q.required(container, "container");
	q.required(meta, "meta");
	q.required(target, "target");
	q.required(targetContainer, "targetContainer");
	container.empty();
	this.appendNew(container);
	this.container = container;
	this.meta = meta;
	this.target = target;
	this.filters = { count: 0 };



	var visual = $("#eidQbVisual");
	var textual = $("#eidQbTextual");
	visual.click(function() {
		textual.removeClass("active");
		visual.addClass("active");
		container.show();
		targetContainer.hide();
	});
	textual.click(function() {
		visual.removeClass("active");
		textual.addClass("active");
		container.hide();
		targetContainer.show();
	});
}
var object = Ogdi.QueryBuilder.prototype;
object.appendNew = function(container) {
	q.required(container, "container");
	var me = this;
	this.root = q.div("filters").appendTo(container);
	var toolbar = q.div().appendTo(container);
	q.icon("plus", "Add a new filter", function() { me.addFilter(); }).appendTo(toolbar);
}

object.dump = function() {    
    var container = this.target;
    if (this.meta.length <= 0) { alert("There are no columns in the dataset."); return false; }

    var text = "";
    var first = true;
    var next = "";
    for (var index = 0; index < this.filters.count; index++) {
        var filter = this.filters["f" + index];
        if (!filter.constant.val()) {
            continue;
        }
        if (first) {
            first = first;
        } else {
            text += " ";
        }
        if (next.length > 0) {
            text += " " + next + " ";
        }
        var info = this.meta[filter.selectedFieldIndex];
        text += info.name + " " + filter.operator.val() + " ";
        if (info.type == "System.String") {
            text += "'" + filter.constant.val() + "'";
        } else {
            text += "" + filter.constant.val() + "";
        }
        next = filter.next.val();
    }
    container.val(text);
}

object.clearFilter = function() {    
    $("img[title='Remove']").each(function() { this.click() });
}

object.addFilter = function() {
    var container = this.root;
    var me = this;

    var filter = {};
    filter.selectedFieldIndex = this.meta.length > 0 ? 0 : -1;
    var layout = filter.layout = {};
    var root = layout.root = q.div("filter").appendTo(container);

    q.icon("up", "Up", function() {
        if (me.filters.count > 0) { // there is anything
            if (filter.index > 0) { // not uppermost
                var above = me.filters["f" + (filter.index - 1)];
                me.filters["f" + filter.index] = above;
                filter.index--;
                me.filters["f" + filter.index] = filter;
                above.index++;

                var parent = me.root[0];
                var removed = parent.removeChild(filter.layout.root[0]);
                parent.insertBefore(removed, above.layout.root[0]);
            }
        }
        me.dump();
    }).appendTo(root);

    q.icon("down", "Down", function() {
        if (me.filters.count > 1) { // there is anything
            if (filter.index < me.filters.count - 1) { // not uppermost
                var below = me.filters["f" + (filter.index + 1)];
                me.filters["f" + filter.index] = below;
                filter.index++;
                me.filters["f" + filter.index] = filter;
                below.index--;

                var parent = me.root[0];
                var removed = parent.removeChild(below.layout.root[0]);
                parent.insertBefore(removed, filter.layout.root[0]);
            }
        }
        me.dump();
    }).appendTo(root);

    this.populateField(filter, root);
    this.populateOperator(filter, root);
    this.populateContant(filter, root);
    this.populateNext(filter, root);

    q.icon("remove", "Remove", function() {
        root.remove();
        for (var index = filter.index + 1; index < me.filters.count; index++) {
            me.filters["f" + (index - 1)] = me.filters["f" + index];
            me.filters["f" + index].index--;
        }
        me.filters.count--;
        delete me.filters["f" + me.filters.count];
        me.dump();
    }).appendTo(root);

    filter.index = this.filters.count++;
    this.filters["f" + filter.index] = filter;

    filter.onSelectedFieldIndexChanged = function(index) {
        filter.selectedFieldIndex = index;
    }

    this.dump();

    return filter;
}

object.populateField = function(filter, container) {
	q.required(filter, "filter");
	q.required(container, "container");
	var meta = this.meta;
	var select = q.tag("select").appendTo(container);
	for (var index = 0; index < meta.length; index++) {
		var info = meta[index];
		var option = new Option();
		var type = info.type.substr(info.type.lastIndexOf(".") + 1);
		option.text = info.name + " (" + type + ")";
		option.value = index;
		select[0].options[select[0].options.length] = option;
	}
	filter.field = select;
	var me = this;
	select.change(function() { filter.onSelectedFieldIndexChanged(select[0].selectedIndex); me.dump(); });
}

object.populateOperator = function(filter, container) {
	q.required(filter, "filter");
	q.required(container, "container");
	var operators = [{ name: "is equal to", value: "eq" }, { name: "is not equal to", value: "ne" }, { name: "is less than", value: "lt" }, { name: "is greater than", value: "gt" }, { name: "is less or equal than", value: "le" }, { name: "is greater or equal than", value: "ge"}];
	var select = q.tag("select").appendTo(container);
	for (var index = 0; index < operators.length; index++) {
		var info = operators[index];
		var option = new Option();
		option.text = info.name;
		option.value = info.value;
		select[0].options[select[0].options.length] = option;
	}
	filter.operator = select;
	var me = this;
	select.change(function() { me.dump(); });
}

object.populateContant = function(filter, container) {
	q.required(filter, "filter");
	q.required(container, "container");
	var input = filter.constant = q.tag("input").appendTo(container);
	var me = this;
	input.keyup(function() { me.dump(); });
}

object.populateNext = function(filter, container) {
	q.required(filter, "filter");
	q.required(container, "container");

	var operators = [{ name: "AND", value: "and" }, { name: "OR", value: "or"}];
	var select = q.tag("select").appendTo(container);
	for (var index = 0; index < operators.length; index++) {
		var info = operators[index];
		var option = new Option();
		option.text = info.name;
		option.value = info.value;
		select[0].options[select[0].options.length] = option;
	}
	filter.next = select;
	var me = this;
	select.change(function() { me.dump(); });

}