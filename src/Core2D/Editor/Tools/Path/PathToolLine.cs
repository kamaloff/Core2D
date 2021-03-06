﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using Core2D.Editor.Tools.Path.Shapes;
using Core2D.Editor.Tools.Selection;
using Core2D.Path.Segments;
using Core2D.Shape;
using Core2D.Shapes;

namespace Core2D.Editor.Tools.Path
{
    /// <summary>
    /// Line path tool.
    /// </summary>
    public class PathToolLine : PathToolBase
    {
        private readonly IServiceProvider _serviceProvider;
        private ToolState _currentState = ToolState.None;
        private XPathLine _line = new XPathLine();
        private LineSelection _selection;

        /// <inheritdoc/>
        public override string Name => "Line";

        /// <summary>
        /// Initialize new instance of <see cref="PathToolLine"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public PathToolLine(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public override void LeftDown(double x, double y)
        {
            base.LeftDown(x, y);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            var pathTool = _serviceProvider.GetService<ToolPath>();
            double sx = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(x, editor.Project.Options.SnapX) : x;
            double sy = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(y, editor.Project.Options.SnapY) : y;
            switch (_currentState)
            {
                case ToolState.None:
                    {
                        _line.Start = editor.TryToGetConnectionPoint(sx, sy) ?? XPoint.Create(sx, sy, editor.Project.Options.PointShape);
                        if (!pathTool.IsInitialized)
                        {
                            pathTool.InitializeWorkingPath(_line.Start);
                        }
                        else
                        {
                            _line.Start = pathTool.GetLastPathPoint();
                        }

                        _line.End = XPoint.Create(sx, sy, editor.Project.Options.PointShape);
                        pathTool.GeometryContext.LineTo(
                            _line.End,
                            editor.Project.Options.DefaultIsStroked,
                            editor.Project.Options.DefaultIsSmoothJoin);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        ToStateOne();
                        Move(null);
                        _currentState = ToolState.One;
                        editor.CancelAvailable = true;
                    }
                    break;
                case ToolState.One:
                    {
                        _line.End.X = sx;
                        _line.End.Y = sy;
                        if (editor.Project.Options.TryToConnect)
                        {
                            var end = editor.TryToGetConnectionPoint(sx, sy);
                            if (end != null)
                            {
                                var figure = pathTool.Geometry.Figures.LastOrDefault();
                                var line = figure.Segments.LastOrDefault() as XLineSegment;
                                line.Point = end;
                            }
                        }

                        _line.Start = _line.End;
                        _line.End = XPoint.Create(sx, sy, editor.Project.Options.PointShape);
                        pathTool.GeometryContext.LineTo(_line.End,
                            editor.Project.Options.DefaultIsStroked,
                            editor.Project.Options.DefaultIsSmoothJoin);
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        Move(null);
                        _currentState = ToolState.One;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void RightDown(double x, double y)
        {
            base.RightDown(x, y);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            var pathTool = _serviceProvider.GetService<ToolPath>();
            switch (_currentState)
            {
                case ToolState.None:
                    break;
                case ToolState.One:
                    {
                        pathTool.RemoveLastSegment<XLineSegment>();

                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(pathTool.Path);
                        Remove();
                        if (pathTool.Path.Geometry.Figures.LastOrDefault().Segments.Length > 0)
                        {
                            Finalize(null);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, pathTool.Path);
                        }
                        else
                        {
                            editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        }
                        pathTool.DeInitializeWorkingPath();
                        _currentState = ToolState.None;
                        editor.CancelAvailable = false;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void Move(double x, double y)
        {
            base.Move(x, y);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            double sx = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(x, editor.Project.Options.SnapX) : x;
            double sy = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(y, editor.Project.Options.SnapY) : y;
            switch (_currentState)
            {
                case ToolState.None:
                    {
                        if (editor.Project.Options.TryToConnect)
                        {
                            editor.TryToHoverShape(sx, sy);
                        }
                    }
                    break;
                case ToolState.One:
                    {
                        if (editor.Project.Options.TryToConnect)
                        {
                            editor.TryToHoverShape(sx, sy);
                        }
                        _line.End.X = sx;
                        _line.End.Y = sy;
                        editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        Move(null);
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ToStateOne()
        {
            base.ToStateOne();
            var editor = _serviceProvider.GetService<ProjectEditor>();
            _selection = new LineSelection(
                editor.Project.CurrentContainer.HelperLayer,
                _line,
                editor.Project.Options.HelperStyle,
                editor.Project.Options.PointShape);

            _selection.ToStateOne();
        }

        /// <inheritdoc/>
        public override void Move(BaseShape shape)
        {
            base.Move(shape);

            _selection.Move();
        }

        /// <inheritdoc/>
        public override void Remove()
        {
            base.Remove();

            _currentState = ToolState.None;

            _selection.Remove();
            _selection = null;
        }
    }
}
