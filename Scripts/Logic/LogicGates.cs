﻿var c = Context.Editor.Container;
var sg = c.StyleGroups.Where(x => x.Name == "Logic").FirstOrDefault();
if (sg == null)
{
    sg = ShapeStyleGroup.Create("Logic");
    c.StyleGroups.Add(sg);
}
var styles = sg.Styles;
var layer = c.CurrentLayer;
var ps = c.PointShape;

var styleTextBig = ShapeStyle.Create(
    "Gate-Big-Text", 
    255, 0, 0, 0, 
    255, 0, 0, 0, 
    2.0, 
    null, 
    "Consolas", 14.0, TextHAlignment.Center, TextVAlignment.Center);
styles.Add(styleTextBig);

var styleLineThick = ShapeStyle.Create(
    "Gate-Line-Thick", 
    255, 0, 0, 0, 
    255, 0, 0, 0, 
    2.0, null, 
    "Consolas", 12.0, TextHAlignment.Center, TextVAlignment.Center);
styles.Add(styleLineThick);

var styleConnector = ShapeStyle.Create(
    "Connector", 
    255, 0, 0, 0, 
    255, 0, 0, 0, 
    1.0, 
    null, 
    "Consolas", 12.0, TextHAlignment.Center, TextVAlignment.Center);
styles.Add(styleConnector);

sg.CurrentStyle = sg.Styles.FirstOrDefault();

{
    var g = XGroup.Create("AND");

    var label = XText.Create(0, 0, 30, 30, styleTextBig, ps, "&", false, "");
    g.Shapes.Add(label);
    
    var frame = XRectangle.Create(0, 0, 30, 30, styleLineThick, ps, false, "");
    g.Shapes.Add(frame);

    var connectorShape = XEllipse.Create(-3, -3, 3, 3, styleConnector, null, true, "");
    
    var cl = XPoint.Create(0, 15, connectorShape, "L");
    cl.State |= ShapeState.Connector;
    g.Connectors.Add(cl);
    
    var cr = XPoint.Create(30, 15, connectorShape, "R");
    cr.State |= ShapeState.Connector;
    g.Connectors.Add(cr);
    
    var ct = XPoint.Create(15, 0, connectorShape, "T");
    ct.State |= ShapeState.Connector;
    g.Connectors.Add(ct);
    
    var cb = XPoint.Create(15, 30, connectorShape, "B");
    cb.State |= ShapeState.Connector;
    g.Connectors.Add(cb);
    
    layer.Shapes.Add(g);
}

{
    var g = XGroup.Create("OR");

    var label = XText.Create(0, 0, 30, 30, styleTextBig, ps, "≥1", false, "");
    g.Shapes.Add(label);
    
    var frame = XRectangle.Create(0, 0, 30, 30, styleLineThick, ps, false, "");
    g.Shapes.Add(frame);

    var connectorShape = XEllipse.Create(-3, -3, 3, 3, styleConnector, null, true, "");
    
    var cl = XPoint.Create(0, 15, connectorShape, "L");
    cl.State |= ShapeState.Connector;
    g.Connectors.Add(cl);
    
    var cr = XPoint.Create(30, 15, connectorShape, "R");
    cr.State |= ShapeState.Connector;
    g.Connectors.Add(cr);
    
    var ct = XPoint.Create(15, 0, connectorShape, "T");
    ct.State |= ShapeState.Connector;
    g.Connectors.Add(ct);
    
    var cb = XPoint.Create(15, 30, connectorShape, "B");
    cb.State |= ShapeState.Connector;
    g.Connectors.Add(cb);
    
    layer.Shapes.Add(g);
}

layer.Invalidate();