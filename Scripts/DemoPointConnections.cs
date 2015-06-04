
var p = Context?.Editor?.Project;
var c = p?.CurrentContainer;
var l1 = XLine.Create(30, 30, 60, 30, p?.CurrentStyleGroup?.CurrentStyle, p?.Options?.PointShape);
var l2 = XLine.Create(60, 30, 60, 60, p?.CurrentStyleGroup?.CurrentStyle, p?.Options?.PointShape);
var l3 = XLine.Create(60, 30, 90, 30, p?.CurrentStyleGroup?.CurrentStyle, p?.Options?.PointShape);

l2.Start = l1.End;
l3.Start = l1.End;

c?.CurrentLayer?.Shapes?.Add(l1);
c?.CurrentLayer?.Shapes?.Add(l2);
c?.CurrentLayer?.Shapes?.Add(l3);
c?.CurrentLayer?.Invalidate();
