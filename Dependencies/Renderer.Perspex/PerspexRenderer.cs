﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Perspex;
using Perspex.Media;
using Perspex.Media.Imaging;
using Core2D;

namespace Dependencies
{
    /// <summary>
    /// Native Perspex shape renderer.
    /// </summary>
    public class PerspexRenderer : Renderer
    {
        private bool _enableImageCache = true;
        private IDictionary<string, Bitmap> _biCache;

        /// <summary>
        /// 
        /// </summary>
        private Func<double, float> _scaleToPage;

        /// <summary>
        /// 
        /// </summary>
        private double _textScaleFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerspexRenderer"/> class.
        /// </summary>
        /// <param name="textScaleFactor"></param>
        public PerspexRenderer(double textScaleFactor = 1.0)
        {
            ClearCache(isZooming: false);

            _textScaleFactor = textScaleFactor;
            _scaleToPage = (value) => (float)(value);
        }

        /// <summary>
        /// Creates a new <see cref="PerspexRenderer"/> instance.
        /// </summary>
        /// <returns>The new instance of the <see cref="PerspexRenderer"/> class.</returns>
        public static Renderer Create()
        {
            return new PerspexRenderer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="rect"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private Point GetTextOrigin(ShapeStyle style, ref Rect2 rect, ref Size size)
        {
            double ox, oy;

            switch (style.TextStyle.TextHAlignment)
            {
                case TextHAlignment.Left:
                    ox = rect.X;
                    break;
                case TextHAlignment.Right:
                    ox = rect.Right - size.Width;
                    break;
                case TextHAlignment.Center:
                default:
                    ox = (rect.Left + rect.Width / 2.0) - (size.Width / 2.0);
                    break;
            }

            switch (style.TextStyle.TextVAlignment)
            {
                case TextVAlignment.Top:
                    oy = rect.Y;
                    break;
                case TextVAlignment.Bottom:
                    oy = rect.Bottom - size.Height;
                    break;
                case TextVAlignment.Center:
                default:
                    oy = (rect.Bottom - rect.Height / 2f) - (size.Height / 2f);
                    break;
            }

            return new Point(ox, oy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Color ToColor(ArgbColor color)
        {
            return Color.FromArgb(
                color.A,
                color.R,
                color.G,
                color.B);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Pen ToPen(BaseStyle style, Func<double, float> scale)
        {
            var lineCap = default(PenLineCap);
            var dashStyle = default(DashStyle);

            switch (style.LineCap)
            {
                case LineCap.Flat:
                    lineCap = PenLineCap.Flat;
                    break;
                case LineCap.Square:
                    lineCap = PenLineCap.Square;
                    break;
                case LineCap.Round:
                    lineCap = PenLineCap.Round;
                    break;
            }

            if (style.Dashes != null)
            {
                dashStyle = new DashStyle(
                    ShapeStyle.ConvertDashesToDoubleArray(style.Dashes),
                    style.DashOffset);
            }

            var pen = new Pen(
                ToSolidBrush(style.Stroke),
                scale(style.Thickness / State.Zoom),
                dashStyle, lineCap,
                lineCap, lineCap);

            return pen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private SolidColorBrush ToSolidBrush(ArgbColor color)
        {
            return new SolidColorBrush(ToColor(color));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="br"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        private static Rect2 CreateRect(XPoint tl, XPoint br, double dx, double dy)
        {
            return Rect2.Create(tl, br, dx, dy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="pen"></param>
        /// <param name="isStroked"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        private static void DrawLineInternal(
            DrawingContext dc,
            Pen pen,
            bool isStroked,
            ref Point p0,
            ref Point p1)
        {
            if (isStroked)
            {
                dc.DrawLine(pen, p0, p1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="line"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        private void DrawLineArrowsInternal(
            DrawingContext dc,
            XLine line,
            double dx,
            double dy,
            out Point pt1,
            out Point pt2)
        {
            Brush fillStartArrow = ToSolidBrush(line.Style.StartArrowStyle.Fill);
            Pen strokeStartArrow = ToPen(line.Style.StartArrowStyle, _scaleToPage);

            Brush fillEndArrow = ToSolidBrush(line.Style.EndArrowStyle.Fill);
            Pen strokeEndArrow = ToPen(line.Style.EndArrowStyle, _scaleToPage);

            double _x1 = line.Start.X + dx;
            double _y1 = line.Start.Y + dy;
            double _x2 = line.End.X + dx;
            double _y2 = line.End.Y + dy;

            XLine.SetMaxLength(line, ref _x1, ref _y1, ref _x2, ref _y2);

            float x1 = _scaleToPage(_x1);
            float y1 = _scaleToPage(_y1);
            float x2 = _scaleToPage(_x2);
            float y2 = _scaleToPage(_y2);

            var sas = line.Style.StartArrowStyle;
            var eas = line.Style.EndArrowStyle;
            double a1 = Math.Atan2(y1 - y2, x1 - x2);
            double a2 = Math.Atan2(y2 - y1, x2 - x1);

            var t1 = MatrixHelper.Rotation(a1, new Vector(x1, y1));
            var t2 = MatrixHelper.Rotation(a2, new Vector(x2, y2));

            pt1 = default(Point);
            pt2 = default(Point);
            double radiusX1 = sas.RadiusX;
            double radiusY1 = sas.RadiusY;
            double sizeX1 = 2.0 * radiusX1;
            double sizeY1 = 2.0 * radiusY1;

            switch (sas.ArrowType)
            {
                default:
                case ArrowType.None:
                    {
                        pt1 = new Point(x1, y1);
                    }
                    break;
                case ArrowType.Rectangle:
                    {
                        pt1 = MatrixHelper.TransformPoint(t1, new Point(x1 - (float)sizeX1, y1));
                        var rect = new Rect2(x1 - sizeX1, y1 - radiusY1, sizeX1, sizeY1);
                        var d = dc.PushPreTransform(t1);
                        DrawRectangleInternal(dc, fillStartArrow, strokeStartArrow, sas.IsStroked, sas.IsFilled, ref rect);
                        d.Dispose();
                    }
                    break;
                case ArrowType.Ellipse:
                    {
                        pt1 = MatrixHelper.TransformPoint(t1, new Point(x1 - (float)sizeX1, y1));
                        var d = dc.PushPreTransform(t1);
                        var rect = new Rect2(x1 - sizeX1, y1 - radiusY1, sizeX1, sizeY1);
                        DrawEllipseInternal(dc, fillStartArrow, strokeStartArrow, sas.IsStroked, sas.IsFilled, ref rect);
                        d.Dispose();
                    }
                    break;
                case ArrowType.Arrow:
                    {
                        var pts = new Point[]
                        {
                            new Point(x1, y1),
                            new Point(x1 - (float)sizeX1, y1 + (float)sizeY1),
                            new Point(x1, y1),
                            new Point(x1 - (float)sizeX1, y1 - (float)sizeY1),
                            new Point(x1, y1)
                        };
                        pt1 = MatrixHelper.TransformPoint(t1, pts[0]);
                        var p11 = MatrixHelper.TransformPoint(t1, pts[1]);
                        var p21 = MatrixHelper.TransformPoint(t1, pts[2]);
                        var p12 = MatrixHelper.TransformPoint(t1, pts[3]);
                        var p22 = MatrixHelper.TransformPoint(t1, pts[4]);
                        DrawLineInternal(dc, strokeStartArrow, sas.IsStroked, ref p11, ref p21);
                        DrawLineInternal(dc, strokeStartArrow, sas.IsStroked, ref p12, ref p22);
                    }
                    break;
            }

            double radiusX2 = eas.RadiusX;
            double radiusY2 = eas.RadiusY;
            double sizeX2 = 2.0 * radiusX2;
            double sizeY2 = 2.0 * radiusY2;

            switch (eas.ArrowType)
            {
                default:
                case ArrowType.None:
                    {
                        pt2 = new Point(x2, y2);
                    }
                    break;
                case ArrowType.Rectangle:
                    {
                        pt2 = MatrixHelper.TransformPoint(t2, new Point(x2 - (float)sizeX2, y2));
                        var rect = new Rect2(x2 - sizeX2, y2 - radiusY2, sizeX2, sizeY2);
                        var d = dc.PushPreTransform(t2);
                        DrawRectangleInternal(dc, fillEndArrow, strokeEndArrow, eas.IsStroked, eas.IsFilled, ref rect);
                        d.Dispose();
                    }
                    break;
                case ArrowType.Ellipse:
                    {
                        pt2 = MatrixHelper.TransformPoint(t2, new Point(x2 - (float)sizeX2, y2));
                        var d = dc.PushPreTransform(t2);
                        var rect = new Rect2(x2 - sizeX2, y2 - radiusY2, sizeX2, sizeY2);
                        DrawEllipseInternal(dc, fillEndArrow, strokeEndArrow, eas.IsStroked, eas.IsFilled, ref rect);
                        d.Dispose();
                    }
                    break;
                case ArrowType.Arrow:
                    {
                        var pts = new Point[]
                        {
                            new Point(x2, y2),
                            new Point(x2 - (float)sizeX2, y2 + (float)sizeY2),
                            new Point(x2, y2),
                            new Point(x2 - (float)sizeX2, y2 - (float)sizeY2),
                            new Point(x2, y2)
                        };
                        pt2 = MatrixHelper.TransformPoint(t2, pts[0]);
                        var p11 = MatrixHelper.TransformPoint(t2, pts[1]);
                        var p21 = MatrixHelper.TransformPoint(t2, pts[2]);
                        var p12 = MatrixHelper.TransformPoint(t2, pts[3]);
                        var p22 = MatrixHelper.TransformPoint(t2, pts[4]);
                        DrawLineInternal(dc, strokeEndArrow, eas.IsStroked, ref p11, ref p21);
                        DrawLineInternal(dc, strokeEndArrow, eas.IsStroked, ref p12, ref p22);
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="brush"></param>
        /// <param name="pen"></param>
        /// <param name="isStroked"></param>
        /// <param name="isFilled"></param>
        /// <param name="rect"></param>
        private static void DrawRectangleInternal(
            DrawingContext dc,
            Brush brush,
            Pen pen,
            bool isStroked,
            bool isFilled,
            ref Rect2 rect)
        {
            if (!isStroked && !isFilled)
                return;

            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);

            if (isFilled)
            {
                dc.FillRectangle(brush, r);
            }

            if (isStroked)
            {
                dc.DrawRectangle(pen, r);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="brush"></param>
        /// <param name="pen"></param>
        /// <param name="isStroked"></param>
        /// <param name="isFilled"></param>
        /// <param name="rect"></param>
        private static void DrawEllipseInternal(
            DrawingContext dc,
            Brush brush,
            Pen pen,
            bool isStroked,
            bool isFilled,
            ref Rect2 rect)
        {
            if (!isFilled && !isStroked)
                return;

            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var g = new EllipseGeometry(r);

            dc.DrawGeometry(
                isFilled ? brush : null,
                isStroked ? pen : null,
                g);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="stroke"></param>
        /// <param name="rect"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="cellWidth"></param>
        /// <param name="cellHeight"></param>
        /// <param name="isStroked"></param>
        private void DrawGridInternal(
            DrawingContext dc,
            Pen stroke,
            ref Rect2 rect,
            double offsetX, double offsetY,
            double cellWidth, double cellHeight,
            bool isStroked)
        {
            double ox = rect.X;
            double oy = rect.Y;
            double sx = ox + offsetX;
            double sy = oy + offsetY;
            double ex = ox + rect.Width;
            double ey = oy + rect.Height;

            for (double x = sx; x < ex; x += cellWidth)
            {
                var p0 = new Point(
                    _scaleToPage(x),
                    _scaleToPage(oy));
                var p1 = new Point(
                    _scaleToPage(x),
                    _scaleToPage(ey));
                DrawLineInternal(dc, stroke, isStroked, ref p0, ref p1);
            }

            for (double y = sy; y < ey; y += cellHeight)
            {
                var p0 = new Point(
                    _scaleToPage(ox),
                    _scaleToPage(y));
                var p1 = new Point(
                    _scaleToPage(ex),
                    _scaleToPage(y));
                DrawLineInternal(dc, stroke, isStroked, ref p0, ref p1);
            }
        }

        /// <inheritdoc/>
        public override void ClearCache(bool isZooming)
        {
            if (!isZooming)
            {
                if (_biCache != null)
                {
                    _biCache.Clear();
                }
                _biCache = new Dictionary<string, Bitmap>();
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XLine line, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            var _dc = dc as DrawingContext;

            Pen strokeLine = ToPen(line.Style, _scaleToPage);
            Point pt1, pt2;

            DrawLineArrowsInternal(_dc, line, dx, dy, out pt1, out pt2);
            DrawLineInternal(_dc, strokeLine, line.IsStroked, ref pt1, ref pt2);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XRectangle rectangle, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            var _dc = dc as DrawingContext;

            Brush brush = ToSolidBrush(rectangle.Style.Fill);
            Pen pen = ToPen(rectangle.Style, _scaleToPage);

            var rect = CreateRect(
                rectangle.TopLeft,
                rectangle.BottomRight,
                dx, dy);

            DrawRectangleInternal(
                _dc,
                brush,
                pen,
                rectangle.IsStroked,
                rectangle.IsFilled,
                ref rect);

            if (rectangle.IsGrid)
            {
                DrawGridInternal(
                    _dc,
                    pen,
                    ref rect,
                    rectangle.OffsetX, rectangle.OffsetY,
                    rectangle.CellWidth, rectangle.CellHeight,
                    true);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XEllipse ellipse, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            var _dc = dc as DrawingContext;

            Brush brush = ToSolidBrush(ellipse.Style.Fill);
            Pen pen = ToPen(ellipse.Style, _scaleToPage);

            var rect = CreateRect(
                ellipse.TopLeft,
                ellipse.BottomRight,
                dx, dy);

            DrawEllipseInternal(
                _dc,
                brush,
                pen,
                ellipse.IsStroked,
                ellipse.IsFilled,
                ref rect);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XArc arc, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            if (!arc.IsFilled && !arc.IsStroked)
                return;

            var _dc = dc as DrawingContext;

            Brush brush = ToSolidBrush(arc.Style.Fill);
            Pen pen = ToPen(arc.Style, _scaleToPage);

            var sg = new StreamGeometry();
            using (var sgc = sg.Open())
            {
                var a = WpfArc.FromXArc(arc, dx, dy);

                sgc.BeginFigure(
                    new Point(a.Start.X, a.Start.Y),
                    arc.IsFilled);

                sgc.ArcTo(
                    new Point(a.End.X, a.End.Y),
                    new Size(a.Radius.Width, a.Radius.Height),
                    0.0,
                    a.IsLargeArc,
                    SweepDirection.Clockwise);

                sgc.EndFigure(false);
            }

            _dc.DrawGeometry(
                arc.IsFilled ? brush : null,
                arc.IsStroked ? pen : null,
                sg);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XBezier bezier, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            if (!bezier.IsFilled && !bezier.IsStroked)
                return;

            var _dc = dc as DrawingContext;

            Brush brush = ToSolidBrush(bezier.Style.Fill);
            Pen pen = ToPen(bezier.Style, _scaleToPage);

            var sg = new StreamGeometry();
            using (var sgc = sg.Open())
            {
                sgc.BeginFigure(
                    new Point(bezier.Point1.X, bezier.Point1.Y),
                    bezier.IsFilled);

                sgc.CubicBezierTo(
                    new Point(bezier.Point2.X, bezier.Point2.Y),
                    new Point(bezier.Point3.X, bezier.Point3.Y),
                    new Point(bezier.Point4.X, bezier.Point4.Y));

                sgc.EndFigure(false);
            }

            _dc.DrawGeometry(
                bezier.IsFilled ? brush : null,
                bezier.IsStroked ? pen : null,
                sg);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XQBezier qbezier, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            if (!qbezier.IsFilled && !qbezier.IsStroked)
                return;

            var _dc = dc as DrawingContext;

            Brush brush = ToSolidBrush(qbezier.Style.Fill);
            Pen pen = ToPen(qbezier.Style, _scaleToPage);

            var sg = new StreamGeometry();
            using (var sgc = sg.Open())
            {
                sgc.BeginFigure(
                    new Point(qbezier.Point1.X, qbezier.Point1.Y),
                    qbezier.IsFilled);

                sgc.QuadraticBezierTo(
                    new Point(qbezier.Point2.X, qbezier.Point2.Y),
                    new Point(qbezier.Point3.X, qbezier.Point3.Y));

                sgc.EndFigure(false);
            }

            _dc.DrawGeometry(
                qbezier.IsFilled ? brush : null,
                qbezier.IsStroked ? pen : null,
                sg);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XText text, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            var _gfx = dc as DrawingContext;

            var tbind = text.BindText(db, r);
            if (string.IsNullOrEmpty(tbind))
                return;

            Brush brush = ToSolidBrush(text.Style.Stroke);

            var fontStyle = Perspex.Media.FontStyle.Normal;
            var fontWeight = Perspex.Media.FontWeight.Normal;
            //var fontDecoration = Perspex.Media.FontDecoration.None;

            if (text.Style.TextStyle.FontStyle != null)
            {
                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(FontStyleFlags.Italic))
                {
                    fontStyle |= Perspex.Media.FontStyle.Italic;
                }

                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(FontStyleFlags.Bold))
                {
                    fontWeight |= Perspex.Media.FontWeight.Bold;
                }

                // TODO: Implement font decoration after Perspex adds support.
                /*
                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(FontStyleFlags.Underline))
                {
                    fontDecoration |= Perspex.Media.FontDecoration.Underline;
                }

                if (text.Style.TextStyle.FontStyle.Flags.HasFlag(FontStyleFlags.Strikeout))
                {
                    fontDecoration |= Perspex.Media.FontDecoration.Strikethrough;
                }
                */
            }

            var ft = new FormattedText(
                tbind,
                text.Style.TextStyle.FontName,
                text.Style.TextStyle.FontSize * _textScaleFactor,
                fontStyle,
                TextAlignment.Left,
                fontWeight);

            var rect = CreateRect(
                text.TopLeft,
                text.BottomRight,
                dx, dy);

            var size = ft.Measure();
            var origin = GetTextOrigin(text.Style, ref rect, ref size);

            _gfx.DrawText(brush, origin, ft);

            ft.Dispose();
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XImage image, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            var _dc = dc as DrawingContext;

            var rect = CreateRect(
                image.TopLeft,
                image.BottomRight,
                dx, dy);

            if (image.IsStroked || image.IsFilled)
            {
                Brush brush = ToSolidBrush(image.Style.Fill);
                Pen pen = ToPen(image.Style, _scaleToPage);

                DrawRectangleInternal(
                    _dc,
                    brush,
                    pen,
                    image.IsStroked,
                    image.IsFilled,
                    ref rect);
            }

            if (_enableImageCache
                && _biCache.ContainsKey(image.Key))
            {
                try
                {
                    var bi = _biCache[image.Key];
                    _dc.DrawImage(
                        bi,
                        1.0,
                        new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                        new Rect(rect.X, rect.Y, rect.Width, rect.Height));
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
            else
            {
                if (State.ImageCache == null || string.IsNullOrEmpty(image.Key))
                    return;

                try
                {
                    var bytes = State.ImageCache.GetImage(image.Key);
                    if (bytes != null)
                    {
                        using (var ms = new System.IO.MemoryStream(bytes))
                        {
                            var bi = new Bitmap(ms);

                            if (_enableImageCache)
                                _biCache[image.Key] = bi;

                            _dc.DrawImage(
                                bi,
                                1.0,
                                new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                                new Rect(rect.X, rect.Y, rect.Width, rect.Height));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, XPath path, double dx, double dy, ImmutableArray<Property> db, Record r)
        {
            if (!path.IsFilled && !path.IsStroked)
                return;

            var _dc = dc as DrawingContext;

            var g = path.Geometry.ToGeometry();

            var brush = ToSolidBrush(path.Style.Fill);
            var pen = ToPen(path.Style, _scaleToPage);
            _dc.DrawGeometry(
                path.IsFilled ? brush : null,
                path.IsStroked ? pen : null,
                g);
        }
    }
}
