﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;
using Perspex.Markup.Xaml;

namespace Core2D.Perspex.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="ShapeControl"/> xaml.
    /// </summary>
    public class ShapeControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeControl"/> class.
        /// </summary>
        public ShapeControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
        }
    }
}