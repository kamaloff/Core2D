﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Core2D.Shapes;

namespace Core2D.Editor.Tools.Path
{
    /// <summary>
    /// Move path tool.
    /// </summary>
    public class PathToolMove : PathToolBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public override string Name => "Move";

        /// <summary>
        /// Initialize new instance of <see cref="PathToolMove"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public PathToolMove(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public override void Move(double x, double y)
        {
            base.Move(x, y);
            var editor = _serviceProvider.GetService<ProjectEditor>();
            double sx = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(x, editor.Project.Options.SnapX) : x;
            double sy = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(y, editor.Project.Options.SnapY) : y;
            if (editor.Project.Options.TryToConnect)
            {
                editor.TryToHoverShape(sx, sy);
            }
        }

        /// <inheritdoc/>
        public override void LeftDown(double x, double y)
        {
            base.LeftDown(x, y);

            var editor = _serviceProvider.GetService<ProjectEditor>();
            var pathTool = _serviceProvider.GetService<ToolPath>();

            editor.CurrentPathTool = pathTool.PreviousPathTool;

            double sx = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(x, editor.Project.Options.SnapX) : x;
            double sy = editor.Project.Options.SnapToGrid ? ProjectEditor.Snap(y, editor.Project.Options.SnapY) : y;
            var start = editor.TryToGetConnectionPoint(sx, sy) ?? XPoint.Create(sx, sy, editor.Project.Options.PointShape);
            pathTool.GeometryContext.BeginFigure(
                    start,
                    editor.Project.Options.DefaultIsFilled,
                    editor.Project.Options.DefaultIsClosed);

            editor.CurrentPathTool.LeftDown(x, y);
        }
    }
}
