﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Immutable;
using Core2D.Shape;

namespace Core2D.Renderer
{
    /// <summary>
    /// Shape renderer state.
    /// </summary>
    public class ShapeRendererState : ObservableObject
    {
        private double _panX;
        private double _panY;
        private double _zoomX;
        private double _zoomY;
        private ShapeState _drawShapeState;
        private BaseShape _selectedShape;
        private ImmutableHashSet<BaseShape> _selectedShapes;
        private IImageCache _imageCache;

        /// <summary>
        /// The X coordinate of current pan position.
        /// </summary>
        public double PanX
        {
            get { return _panX; }
            set { Update(ref _panX, value); }
        }

        /// <summary>
        /// The Y coordinate of current pan position.
        /// </summary>
        public double PanY
        {
            get { return _panY; }
            set { Update(ref _panY, value); }
        }

        /// <summary>
        /// The X component of current zoom value.
        /// </summary>
        public double ZoomX
        {
            get { return _zoomX; }
            set { Update(ref _zoomX, value); }
        }

        /// <summary>
        /// The Y component of current zoom value.
        /// </summary>
        public double ZoomY
        {
            get { return _zoomY; }
            set { Update(ref _zoomY, value); }
        }

        /// <summary>
        /// Flag indicating shape state to enable its drawing.
        /// </summary>
        public ShapeState DrawShapeState
        {
            get { return _drawShapeState; }
            set { Update(ref _drawShapeState, value); }
        }

        /// <summary>
        /// Currently selected shape.
        /// </summary>
        public BaseShape SelectedShape
        {
            get { return _selectedShape; }
            set { Update(ref _selectedShape, value); }
        }

        /// <summary>
        /// Currently selected shapes.
        /// </summary>
        public ImmutableHashSet<BaseShape> SelectedShapes
        {
            get { return _selectedShapes; }
            set { Update(ref _selectedShapes, value); }
        }

        /// <summary>
        /// Image cache repository.
        /// </summary>
        public IImageCache ImageCache
        {
            get { return _imageCache; }
            set { Update(ref _imageCache, value); }
        }

        /// <summary>
        /// Initializes a new <see cref="ShapeRendererState"/> instance.
        /// </summary>
        public ShapeRendererState()
        {
            _panX = 0.0;
            _panY = 0.0;
            _zoomX = 1.0;
            _zoomY = 1.0;
            _drawShapeState = ShapeState.Create(ShapeStateFlags.Visible);
            _selectedShape = default(BaseShape);
            _selectedShapes = default(ImmutableHashSet<BaseShape>);
        }
    }
}
