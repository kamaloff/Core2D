﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Perspex;
using Perspex.Controls;
using Perspex.VisualTree;
using Perspex.Xaml.Interactivity;
using System;

namespace Core2D.Perspex.Interactions.Behaviors
{
    public class BindTagToVisualRootDataContextBehavior : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AttachedToVisualTree += AttachedToVisualTree;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.AttachedToVisualTree -= AttachedToVisualTree;
        }

        private void AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            BindDataContextToTag((IControl)AssociatedObject.GetVisualRoot(), AssociatedObject);
        }

        private static void BindDataContextToTag(IControl source, IControl target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var data = source.GetObservable(Control.DataContextProperty);
            if (data != null)
            {
                target.Bind(Control.TagProperty, data);
            }
        }
    }
}